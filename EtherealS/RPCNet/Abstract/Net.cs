using EtherealS.Core.Delegates;
using EtherealS.Core.Enums;
using EtherealS.Core.Model;
using EtherealS.NativeServer;
using EtherealS.RPCRequest;
using EtherealS.RPCService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace EtherealS.RPCNet
{
    public abstract class Net:INet
    {
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
        protected ConcurrentDictionary<string, Service> services = new ConcurrentDictionary<string, Service>();
        /// <summary>
        /// Reqeust映射表
        /// </summary>
        protected Dictionary<string, Request> requests = new Dictionary<string, Request>();
        protected NetConfig config;
        /// <summary>
        /// Server
        /// </summary>
        protected NativeServer.Abstract.Server server;
        /// <summary>
        /// Net网关名
        /// </summary>
        protected string name;
        protected NetType netType;
        #endregion

        #region --属性--
        public ConcurrentDictionary<object, Token> Tokens { get => tokens; set => tokens = value; }
        public NetConfig Config { get => config; set => config = value; }
        public ConcurrentDictionary<string, Service> Services { get => services; }
        public Dictionary<string, Request> Requests { get => requests; }
        public string Name { get => name; set => name = value; }
        public NativeServer.Abstract.Server Server { get => server; set => server = value; }
        public NetType NetType { get => netType; set => netType = value; }

        #endregion

        #region --方法--
        /// <summary>
        /// 部署节点
        /// </summary>
        /// <returns></returns>
        public abstract bool Publish();

        public ClientResponseModel ClientRequestReceiveProcess(Token token, ClientRequestModel request)
        {
            if (Services.TryGetValue(request.Service, out Service service))
            {
                if (service.Methods.TryGetValue(request.MethodId, out MethodInfo method))
                {
                    string log = "--------------------------------------------------\n" +
                        $"{DateTime.Now}::{name}::[客-请求]\n{request}\n" +
                        "--------------------------------------------------\n";
                    OnLog(RPCLog.LogCode.Runtime, log);
                    if (Config.OnInterceptor(service, method, token) &&
                        service.Config.OnInterceptor(service, method, token))
                    {
                        string[] params_id = request.MethodId.Split('-');
                        for (int i = 1; i < params_id.Length; i++)
                        {
                            if (service.Config.Types.TypesByName.TryGetValue(params_id[i], out RPCType type))
                            {
                                request.Params[i] = type.Deserialize((string)request.Params[i]);
                            }
                            else throw new RPCException($"RPC中的{params_id[i]}类型中尚未被注册");
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
                            service.Config.Types.TypesByType.TryGetValue(return_type, out RPCType type);
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

        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e)
        {
            if (exceptionEvent != null)
            {
                if (e is not RPCException)
                {
                    e = new RPCException(e);
                }
                (e as RPCException).Net = this;
                exceptionEvent?.Invoke(e);
            }
        }

        public void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }

        public void OnLog(RPCLog log)
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
