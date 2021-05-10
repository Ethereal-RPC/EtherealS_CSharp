using EtherealS.Model;
using EtherealS.RPCRequest;
using EtherealS.RPCService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EtherealS.RPCNet
{
    public class Net
    {
        #region --委托--
        public delegate void ClientRequestReceiveDelegate(BaseUserToken token, ClientRequestModel request);
        public delegate void ServerRequestSendDelegate(BaseUserToken token, ServerRequestModel request);
        public delegate void ClientResponseSendDelegate(BaseUserToken token, ClientResponseModel response);

        #endregion

        #region --字段--
        /// <summary>
        /// Token映射表
        /// </summary>
        private ConcurrentDictionary<object, BaseUserToken> tokens = new ConcurrentDictionary<object, BaseUserToken>();
        /// <summary>
        /// Service映射表
        /// </summary>
        private ConcurrentDictionary<string, Service> services = new ConcurrentDictionary<string, Service>();
        /// <summary>
        /// Reqeust映射表
        /// </summary>
        private Dictionary<string, Request> requests = new Dictionary<string, Request>();
        private NetConfig config;
        /// <summary>
        /// 收到客户端请求委托实现
        /// </summary>
        private ClientRequestReceiveDelegate clientRequestReceive;
        /// <summary>
        /// 发送服务器请求委托实现
        /// </summary>
        private ServerRequestSendDelegate serverRequestSend;
        /// <summary>
        /// 客户端请求返回委托实现
        /// </summary>
        private ClientResponseSendDelegate clientResponseSend;
        private Tuple<string, string> serverKey;
        #endregion

        #region --属性--
        public ConcurrentDictionary<object, BaseUserToken> Tokens { get => tokens; set => tokens = value; }
        public NetConfig Config { get => config; set => config = value; }
        public ClientRequestReceiveDelegate ClientRequestReceive { get => clientRequestReceive; set => clientRequestReceive = value; }
        public ServerRequestSendDelegate ServerRequestSend { get => serverRequestSend; set => serverRequestSend = value; }
        public ClientResponseSendDelegate ClientResponseSend { get => clientResponseSend; set => clientResponseSend = value; }
        public ConcurrentDictionary<string, Service> Services { get => services; }
        public Dictionary<string, Request> Requests { get => requests; }
        public Tuple<string, string> ServerKey { get => serverKey; set => serverKey = value; }
        #endregion

        #region --方法--
        public Net()
        {
            clientRequestReceive = ClientRequestReceiveProcess;
        }

        private void ClientRequestReceiveProcess(BaseUserToken token, ClientRequestModel request)
        {
            if (Services.TryGetValue(request.Service, out Service service))
            {
                if (service.Methods.TryGetValue(request.MethodId, out MethodInfo method))
                {
                    string log = "--------------------------------------------------\n" +
                        $"{DateTime.Now}::{serverKey.Item1}:{serverKey.Item2}::[客-请求]\n{request}\n" +
                        "--------------------------------------------------\n";
                    service.Config.OnLog(RPCLog.LogCode.Runtime, log);
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
                            else service.Config.OnException(new RPCException($"RPC中的{params_id[i]}类型中尚未被注册"));
                        }

                        if (method.GetParameters().Length == request.Params.Length) request.Params[0] = token;
                        else if (request.Params.Length > 1)
                        {
                            object[] new_params = new object[request.Params.Length - 1];
                            for (int i = 0; i < new_params.Length; i++)
                            {
                                new_params[i] = request.Params[i + 1];
                                request.Params = new_params;
                            }
                        }

                        object result = method.Invoke(service.Instance, request.Params);
                        Type return_type = method.ReturnType;
                        if (return_type != typeof(void))
                        {
                            service.Config.Types.TypesByType.TryGetValue(return_type, out RPCType type);
                            ClientResponseSend(token, new ClientResponseModel("2.0", type.Serialize(result), type.Name, request.Id, request.Service, null));
                        }
                    }
                }
                else service.Config.OnException(new RPCException(RPCException.ErrorCode.RuntimeError, $"未找到方法[{serverKey.Item1}:{serverKey.Item2}:{request.Service}:{request.MethodId}]"));
            }
            else Config.OnException(new RPCException(RPCException.ErrorCode.RuntimeError, $"未找到服务[{serverKey.Item1}:{serverKey.Item2}:{request.Service}]"));
        }
        #endregion
    }
}
