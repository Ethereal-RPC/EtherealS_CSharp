using EtherealS.Model;
using EtherealS.NativeServer;
using EtherealS.RPCRequest;
using EtherealS.RPCService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using EtherealS.RPCNet.Client.Request;
using EtherealS.RPCNet.Model;
using EtherealS.RPCNet.Client.Service;
using EtherealS.RPCNet.Server.Service;
using EtherealS.RPCNet.Server.Request;

namespace EtherealS.RPCNet
{
    public class Net
    {
        #region --委托--
        public delegate void ClientRequestReceiveDelegate(BaseUserToken token, ClientRequestModel request);
        public delegate void ServerRequestSendDelegate(BaseUserToken token, ServerRequestModel request);
        public delegate void ClientResponseSendDelegate(BaseUserToken token, ClientResponseModel response);
        public delegate void OnLogDelegate(RPCLog log, Net net);
        public delegate void OnExceptionDelegate(Exception exception, Net net);
        #endregion


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
        /// <summary>
        /// Server
        /// </summary>
        private ServerListener server;
        /// <summary>
        /// Net网关名
        /// </summary>
        private string name;
        #endregion

        #region --属性--
        public ConcurrentDictionary<object, BaseUserToken> Tokens { get => tokens; set => tokens = value; }
        public NetConfig Config { get => config; set => config = value; }
        public ClientRequestReceiveDelegate ClientRequestReceive { get => clientRequestReceive; set => clientRequestReceive = value; }
        public ServerRequestSendDelegate ServerRequestSend { get => serverRequestSend; set => serverRequestSend = value; }
        public ClientResponseSendDelegate ClientResponseSend { get => clientResponseSend; set => clientResponseSend = value; }
        public ConcurrentDictionary<string, Service> Services { get => services; }
        public Dictionary<string, Request> Requests { get => requests; }
        public string Name { get => name; set => name = value; }
        public ServerListener Server { get => server; set => server = value; }

        #endregion

        #region --方法--
        public Net()
        {
            clientRequestReceive = ClientRequestReceiveProcess;
        }
        /// <summary>
        /// 部署节点
        /// </summary>
        /// <returns></returns>
        public bool Publish()
        {
            if (config.NetNodeMode)//开启分布式模式
            {
                #region --Server--
                {
                    //注册数据类型
                    RPCTypeConfig types = new RPCTypeConfig();
                    types.Add<int>("Int");
                    types.Add<long>("Long");
                    types.Add<string>("String");
                    types.Add<bool>("Bool");
                    types.Add<NetNode>("NetNode");
                    //注册服务
                    ServerNodeService serverDistributeService = (ServerNodeService)ServiceCore.Register<ServerNodeService>(this, "ServerNetNodeService", types).Instance;
                    //注册请求
                    serverDistributeService.DistributeRequest = RequestCore.Register<ClientNodeRequest>(this, "ClientNetNodeService", types);
                }
                #endregion

                #region --Client--
                {
                    Thread thread = new Thread((cycle)=> {
                        while (true)
                        {
                            foreach (Tuple<string, string, EtherealC.NativeClient.ClientConfig> item in config.NetNodeIps)
                            {
                                string ip = item.Item1;
                                string port = item.Item2;
                                EtherealC.NativeClient.ClientConfig clientConfig = item.Item3;
                                if (ip == server.ServerKey.Item1 && port == server.ServerKey.Item2) continue; //该地址就是本机地址，已经以服务器方式启动
                                //发现已存在，即已经建立连接
                                if (EtherealC.RPCNet.NetCore.Get($"NetNodeClient-{ip}-{port}", out EtherealC.RPCNet.Net temp)) continue;
                                //tip:S项目引入C，对于C的引用，命名必须全部采取全名引用，避免冲突。
                                //注册数据类型
                                EtherealC.Model.RPCTypeConfig types = new EtherealC.Model.RPCTypeConfig();
                                types.Add<int>("Int");
                                types.Add<long>("Long");
                                types.Add<string>("String");
                                types.Add<bool>("Bool");
                                types.Add<NetNode>("NetNode");
                                EtherealC.RPCNet.Net net = EtherealC.RPCNet.NetCore.Register($"NetNodeClient-{ip}-{port}");
                                //注册服务
                                ClientNodeService clientNodeService = (ClientNodeService)EtherealC.RPCService.ServiceCore.Register<ClientNodeService>(net, "ClientNetNodeService",types).Instance;
                                //注册请求
                                clientNodeService.ServerNodeRequest = EtherealC.RPCRequest.RequestCore.Register<ServerNodeRequest>(net, "ServerNetNodeService", types);
                                //注册连接
                                EtherealC.NativeClient.SocketClient client = EtherealC.NativeClient.ClientCore.Register(net, ip, port, clientConfig);

                                net.LogEvent += ClientNetLog;
                                net.ExceptionEvent += ClientNetException;

                                client.ConnectFailEvent += Config_ConnectFailEvent;
                                client.ConnectSuccessEvent += Config_ConnectSuccessEvent;
                                //部署
                                net.Publish();
                            }
                            Thread.Sleep((int)cycle);
                        }
                    });
                    thread.Start(config.NetNodeHeartbeatCycle);
                }
                #endregion
            }
            server.Start();
            return true;
        }

        internal void OnServerException(Exception exception, ServerListener server)
        {
            OnException(exception);
        }

        internal void OnServerLog(RPCLog log, ServerListener server)
        {
            OnLog(log);
        }

        private void ClientNetException(Exception exception, EtherealC.RPCNet.Net net)
        {
            OnException(RPCException.ErrorCode.Runtime, exception.Message);
        }

        private void ClientNetLog(EtherealC.Model.RPCLog log, EtherealC.RPCNet.Net net)
        {
            OnLog(RPCLog.LogCode.Runtime,log.Message);
        }

        internal void OnRequestException(Exception exception, Request request)
        {
            OnException(exception);
        }

        internal void OnRequestLog(RPCLog log, Request request)
        {
            OnLog(log);
        }

        internal void OnServiceException(Exception exception, Service service)
        {
            OnException(exception);
        }

        internal void OnServiceLog(RPCLog log, Service service)
        {
            OnLog(log);
        }


        private void Config_ConnectSuccessEvent(EtherealC.NativeClient.SocketClient client)
        {
            //注册节点信息
            if (EtherealC.RPCRequest.RequestCore.Get($"NetNodeClient-{client.ClientKey.Item1}-{client.ClientKey.Item2}", "ServerNetNodeService", out EtherealC.RPCRequest.Request serverDistributeRequest))
            {
                //生成节点信息
                NetNode node = new NetNode();
                node.Ip = server.ServerKey.Item1;
                node.Port = server.ServerKey.Item2;
                node.Name = $"{name}-{node.Ip}-{node.Port}";
                node.Connects = server.NumConnectedSockets;
                node.HardwareInformation = new HardwareInformation();
                node.HardwareInformation.NetworkInterfaces = Utils.NetworkInterfaceHelper.GetAllNetworkInterface();
                node.HardwareInformation.Is64BitOperatingSystem = Environment.Is64BitOperatingSystem.ToString();
                node.HardwareInformation.ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString();
                node.HardwareInformation.OSArchitecture = RuntimeInformation.OSArchitecture.ToString();
                node.HardwareInformation.OSDescription = RuntimeInformation.OSDescription.ToString();
                node.Services = new Dictionary<string, ServiceNode>();
                node.Requests = new Dictionary<string, RequestNode>();
                //添加服务信息
                foreach (Service service in services.Values)
                {
                    ServiceNode serviceNode = new ServiceNode();
                    serviceNode.Name = service.Name;
                    node.Services.Add(serviceNode.Name, serviceNode);
                }
                //添加请求信息
                foreach (Request request in requests.Values)
                {
                    RequestNode requestNode = new RequestNode();
                    requestNode.Name = request.Name;
                    node.Requests.Add(requestNode.Name, requestNode);
                }
                ((ServerNodeRequest)serverDistributeRequest).Register(node);
                //向目标主机注册节点信息
                if (true)
                {
                    OnLog(RPCLog.LogCode.Runtime, $"分布式节点：{client.ClientKey.Item1}-{client.ClientKey.Item2}连接成功");
                }
            }
            else OnException(RPCException.ErrorCode.Runtime, $"EtherealC中未找到 NetNodeClient-{client.ClientKey.Item1}-{client.ClientKey.Item2}-ServerNodeService");
        }

        private void Config_ConnectFailEvent(EtherealC.NativeClient.SocketClient client)
        {
            Console.WriteLine("已断开连接");
            EtherealC.NativeClient.ClientCore.UnRegister(client.NetName,client.ServiceName);
        }

        private void ClientRequestReceiveProcess(BaseUserToken token, ClientRequestModel request)
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
                            else OnException(new RPCException($"RPC中的{params_id[i]}类型中尚未被注册"));
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
                else
                {
                    ClientResponseSend(token, new ClientResponseModel("2.0", null,null, request.Id, request.Service, new Error(Error.ErrorCode.NotFoundMethod, $"未找到方法[{name}:{request.Service}:{request.MethodId}]",null))); ;
                }
            }
            else
            {
                ClientResponseSend(token, new ClientResponseModel("2.0", null, null, request.Id, request.Service, new Error(Error.ErrorCode.NotFoundService, $"未找到服务[{name}:{request.Service}]",null))); ;
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
                exceptionEvent.Invoke(e, this);
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
                logEvent.Invoke(log, this);
            }
        }
        #endregion
    }
}
