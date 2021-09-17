using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using EtherealS.Core.Delegates;
using EtherealS.Core.Model;
using EtherealS.Net.Interface;
using EtherealS.Server.Abstract;

namespace EtherealS.Net.Abstract
{
    public abstract class Net:INet
    {
        public enum NetType { WebSocket }
        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
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
        protected Dictionary<string, Request.Abstract.Request> requests = new Dictionary<string, Request.Abstract.Request>();
        protected NetConfig config;
        /// <summary>
        /// Server
        /// </summary>
        protected Server.Abstract.Server server;
        /// <summary>
        /// Net网关名
        /// </summary>
        protected string name;
        protected NetType type;
        #endregion

        #region --属性--
        public ConcurrentDictionary<object, Token> Tokens { get => tokens; set => tokens = value; }
        public NetConfig Config { get => config; set => config = value; }
        public ConcurrentDictionary<string, Service.Abstract.Service> Services { get => services; }
        public Dictionary<string, Request.Abstract.Request> Requests { get => requests; }
        public string Name { get => name; set => name = value; }
        public Server.Abstract.Server Server { get => server; set => server = value; }
        public NetType Type { get => type; set => type = value; }

        #endregion

        #region --方法--
        /// <summary>
        /// 部署节点
        /// </summary>
        /// <returns></returns>
        public abstract bool Publish();

        public ClientResponseModel ClientRequestReceiveProcess(Token token, ClientRequestModel request)
        {
            if (Services.TryGetValue(request.Service, out Service.Abstract.Service service))
            {
                if (service.Methods.TryGetValue(request.MethodId, out MethodInfo method))
                {
                    string log = "--------------------------------------------------\n" +
                        $"{DateTime.Now}::{name}::[客-请求]\n{request}\n" +
                        "--------------------------------------------------\n";
                    OnLog(TrackLog.LogCode.Runtime, log);
                    if (Config.OnInterceptor(service, method, token) &&
                        service.Config.OnInterceptor(service, method, token))
                    {
                        string[] params_id = request.MethodId.Split('-');
                        for (int i = 1; i < params_id.Length; i++)
                        {
                            if (service.Config.Types.TypesByName.TryGetValue(params_id[i], out AbstractType type))
                            {
                                request.Params[i] = type.Deserialize((string)request.Params[i]);
                            }
                            else throw new TrackException($"RPC中的{params_id[i]}类型中尚未被注册");
                        }

                        if (method.GetParameters().Length == request.Params.Length) request.Params[0] = token;
                        else if (request.Params.Length > 1)
                        {
                            object[] new_params = new object[request.Params.Length - 1];
                            for (int i = 0; i < new_params.Length; i++)
                            {
                                new_params[i] = request.Params[i + 1];
                            }
                            request.Params = new_params;
                        }

                        object result = method.Invoke(service.Instance, request.Params);
                        Type return_type = method.ReturnType;
                        if (return_type != typeof(void))
                        {
                            service.Config.Types.TypesByType.TryGetValue(return_type, out AbstractType type);
                            return new ClientResponseModel(type.Serialize(result), type.Name, request.Id, request.Service, null);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else return new ClientResponseModel(null, null, request.Id, request.Service, new Error(Error.ErrorCode.Intercepted, $"请求已被拦截", null));
                }
                else
                {
                    return new ClientResponseModel(null,null, request.Id, request.Service, new Error(Error.ErrorCode.NotFoundMethod, $"未找到方法[{name}:{request.Service}:{request.MethodId}]",null));
                }
            }
            else
            {
                return new ClientResponseModel(null, null, request.Id, request.Service, new Error(Error.ErrorCode.NotFoundService, $"未找到服务[{name}:{request.Service}]",null));
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

        #endregion
    }
}
