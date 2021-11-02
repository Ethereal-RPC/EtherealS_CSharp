using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using EtherealS.Core.Delegates;
using EtherealS.Core.Model;
using EtherealS.Net.Extension.Plugins;
using EtherealS.Net.Interface;
using EtherealS.Server.Abstract;
using EtherealS.Service.Attribute;

namespace EtherealS.Net.Abstract
{
    public abstract class Net:INet
    {
        public enum NetType { WebSocket }

        #region --事件字段--
        public delegate bool InterceptorDelegate(Net net,Service.Abstract.Service service, MethodInfo method, Token token);
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
        /// <summary>
        /// 拦截器事件
        /// </summary>
        public event InterceptorDelegate InterceptorEvent;
        /// <summary>
        /// 日志输出事件
        /// </summary>
        public event OnLogDelegate LogEvent
        {
            add
            {
                logEvent -= value;
                logEvent += value;
            }
            remove
            {
                logEvent -= value;
            }
        }
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent
        {
            add
            {
                exceptionEvent -= value;
                exceptionEvent += value;
            }
            remove
            {
                exceptionEvent -= value;
            }

        }

        #endregion

        #region --字段--
        /// <summary>
        /// Token映射表
        /// </summary>
        protected ConcurrentDictionary<object, Token> tokens = new ConcurrentDictionary<object, Token>();
        /// <summary>
        /// Service映射表
        /// </summary>
        protected ConcurrentDictionary<string, Service.Abstract.Service> services = new ConcurrentDictionary<string, Service.Abstract.Service>();
        /// <summary>
        /// Reqeust映射表
        /// </summary>
        protected ConcurrentDictionary<string, Request.Abstract.Request> requests = new ConcurrentDictionary<string, Request.Abstract.Request>();
        protected NetConfig config;
        /// <summary>
        /// Server
        /// </summary>
        protected Server.Abstract.Server server;
        /// <summary>
        /// Net网关名
        /// </summary>
        protected string name;
        /// <summary>
        /// Net类型
        /// </summary>
        protected NetType type;
        /// <summary>
        /// 插件管理器
        /// </summary>
        protected PluginManager pluginManager;
        #endregion

        #region --属性--
        public ConcurrentDictionary<object, Token> Tokens { get => tokens; set => tokens = value; }
        public NetConfig Config { get => config; set => config = value; }
        public ConcurrentDictionary<string, Service.Abstract.Service> Services { get => services; }
        public ConcurrentDictionary<string, Request.Abstract.Request> Requests { get => requests; }
        public string Name { get => name; set => name = value; }
        public Server.Abstract.Server Server { get => server; set => server = value; }
        public NetType Type { get => type; set => type = value; }
        protected PluginManager PluginManager { get => pluginManager; set => pluginManager = value; }

        #endregion

        #region --方法--
        /// <summary>
        /// 部署节点
        /// </summary>
        /// <returns></returns>
        public virtual bool Publish()
        {
            try
            {
                if (config.PluginMode)
                {
                    pluginManager.Listen();
                }
                server.Start();
            }
            catch (TrackException e)
            {
                OnException(e);
            }
            catch (Exception e)
            {
                OnException(new TrackException(e));
            }
            return true;
        }

        public Net(string name)
        {
            this.name = name;
            pluginManager = new PluginManager(name);
        }
        public ClientResponseModel ClientRequestReceiveProcess(Token token, ClientRequestModel request)
        {
            try
            {
                if (Services.TryGetValue(request.Service, out Service.Abstract.Service service))
                {
                    if (service.Methods.TryGetValue(request.MethodId, out MethodInfo method))
                    {
                        string log = "--------------------------------------------------\n" +
                            $"{DateTime.Now}::{name}::[客-请求]\n{request}\n" +
                            "--------------------------------------------------\n";
                        OnLog(TrackLog.LogCode.Runtime, log);
                        if (OnInterceptor(service, method, token) &&
                            service.OnInterceptor(this, method, token))
                        {
                            ParameterInfo[] parameterInfos = method.GetParameters();
                            List<object> parameters = new List<object>(parameterInfos.Length);
                            int i = 0;
                            foreach (ParameterInfo parameterInfo in parameterInfos)
                            {
                                if (parameterInfo.GetCustomAttribute<Server.Attribute.Token>(true) != null)
                                {
                                    parameters.Add(token);
                                }
                                else if (service.Types.TypesByType.TryGetValue(parameterInfo.ParameterType, out AbstractType type)
                                    || service.Types.TypesByName.TryGetValue(parameterInfo.GetCustomAttribute<Core.Attribute.AbstractType>(true)?.AbstractName, out type))
                                {
                                    parameters.Add(type.Deserialize(request.Params[i++]));
                                }
                                else return new ClientResponseModel(null, request.Id, request.Service, new Error(Error.ErrorCode.Intercepted, $"RPC中的{request.Params[i]}类型中尚未被注册", null));
                            }
                            object result = method.Invoke(service, parameters.ToArray());
                            Type return_type = method.ReturnType;
                            if (return_type != typeof(void))
                            {
                                if (service.Types.TypesByType.TryGetValue(return_type, out AbstractType type)
                                || service.Types.TypesByName.TryGetValue(method.GetCustomAttribute<Core.Attribute.AbstractType>(true)?.AbstractName, out type))
                                {
                                    return new ClientResponseModel(type.Serialize(result), request.Id, request.Service, null);
                                }
                                else return new ClientResponseModel(null, request.Id, request.Service, new Error(Error.ErrorCode.Intercepted, $"RPC中的{return_type}类型中尚未被注册", null));
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return new ClientResponseModel(null, request.Id, request.Service, new Error(Error.ErrorCode.Intercepted, $"请求已被拦截", null));
                        }
                    }
                    else
                    {
                        return new ClientResponseModel(null, request.Id, request.Service, new Error(Error.ErrorCode.NotFoundMethod, $"未找到方法[{name}:{request.Service}:{request.MethodId}]", null));
                    }
                }
                else
                {
                    return new ClientResponseModel(null, request.Id, request.Service, new Error(Error.ErrorCode.NotFoundService, $"未找到服务[{name}:{request.Service}]", null));
                }
            }
            catch(Exception e)
            {
                return new ClientResponseModel(null, request.Id, request.Service, new Error(Error.ErrorCode.Common, $"{e.Message}\n {e.StackTrace}", null));
            }
        }

        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            if (exceptionEvent != null)
            {
                e.Net = this;
                exceptionEvent?.Invoke(e);
            }
        }

        public void OnLog(TrackLog.LogCode code, string message)
        {
            OnLog(new TrackLog(code, message));
        }

        public void OnLog(TrackLog log)
        {
            if (logEvent != null)
            {
                log.Net = this;
                logEvent?.Invoke(log);
            }
        }
        public bool OnInterceptor(Service.Abstract.Service service, MethodInfo method, Token token)
        {
            if (InterceptorEvent != null)
            {
                foreach (InterceptorDelegate item in InterceptorEvent.GetInvocationList())
                {
                    if (!item.Invoke(this,service, method, token)) return false;
                }
                return true;
            }
            else return true;
        }
        #endregion
    }
}
