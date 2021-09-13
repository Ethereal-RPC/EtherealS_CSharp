using EtherealS.Model;
using EtherealS.NativeServer;
using EtherealS.RPCNet.NetNodeClient.Request;
using EtherealS.RPCNet.NetNodeClient.Service;
using EtherealS.RPCNet.NetNodeModel;
using EtherealS.RPCNet.Server.NetNodeRequest;
using EtherealS.RPCNet.Server.NetNodeService;
using EtherealS.RPCRequest;
using EtherealS.RPCService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace EtherealS.RPCNet
{
    public class Net
    {
        #region --委托--
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
        private ConcurrentDictionary<object, BaseToken> tokens = new ConcurrentDictionary<object, BaseToken>();
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
        /// Server
        /// </summary>
        private NativeServer.Server server;
        /// <summary>
        /// Net网关名
        /// </summary>
        private string name;
        #endregion

        #region --属性--
        public ConcurrentDictionary<object, BaseToken> Tokens { get => tokens; set => tokens = value; }
        public NetConfig Config { get => config; set => config = value; }
        public ConcurrentDictionary<string, Service> Services { get => services; }
        public Dictionary<string, Request> Requests { get => requests; }
        public string Name { get => name; set => name = value; }
        public NativeServer.Server Server { get => server; set => server = value; }

        #endregion

        #region --方法--
        public Net()
        {

        }
        /// <summary>
        /// 部署节点
        /// </summary>
        /// <returns></returns>
        public bool Publish()
        {
            try
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
                        foreach (Tuple<string, EtherealC.NativeClient.ClientConfig> item in config.NetNodeIps)
                        {
                            string prefixes = item.Item1;
                            EtherealC.NativeClient.ClientConfig clientConfig = item.Item2;
                            if (EtherealC.RPCRequest.RequestCore.Get($"NetNodeClient-{prefixes}", "ServerNetNodeService", out EtherealC.RPCRequest.Request temp) && temp.Client != null) continue;
                            //tip:S项目引入C，对于C的引用，命名必须全部采取全名引用，避免冲突。
                            //注册数据类型
                            EtherealC.Model.RPCTypeConfig types = new EtherealC.Model.RPCTypeConfig();
                            types.Add<int>("Int");
                            types.Add<long>("Long");
                            types.Add<string>("String");
                            types.Add<bool>("Bool");
                            types.Add<NetNode>("NetNode");
                            EtherealC.RPCNet.Net net = EtherealC.RPCNet.NetCore.Register($"NetNodeClient-{prefixes}");
                            net.Config.NetNodeMode = false;
                            //注册服务
                            ClientNodeService clientNodeService = (ClientNodeService)EtherealC.RPCService.ServiceCore.Register<ClientNodeService>(net, "ClientNetNodeService", types).Instance;
                            //注册请求
                            clientNodeService.ServerNodeRequest = EtherealC.RPCRequest.RequestCore.Register<ServerNodeRequest>(net, "ServerNetNodeService", types);
                            net.LogEvent += ClientNetLog;
                            net.ExceptionEvent += ClientNetException;
                        }
                        Thread thread = new Thread((cycle) => {
                            try
                            {
                                while (true)
                                {
                                    foreach (Tuple<string, EtherealC.NativeClient.ClientConfig> item in config.NetNodeIps)
                                    {
                                        string prefixes = item.Item1;
                                        EtherealC.NativeClient.ClientConfig clientConfig = item.Item2;
                                        if (!EtherealC.RPCNet.NetCore.Get($"NetNodeClient-{prefixes}", out EtherealC.RPCNet.Net net))
                                        {
                                            throw new RPCException(RPCException.ErrorCode.Runtime, $"NetNode-Client-未找到Net:NetNodeClient-{prefixes}");
                                        }
                                        if (EtherealC.RPCRequest.RequestCore.Get(net, "ServerNetNodeService", out EtherealC.RPCRequest.Request serverNodeRequest))
                                        {
                                            if (serverNodeRequest.Client != null)
                                            {
                                                if (serverNodeRequest.Client.Accept.State != System.Net.WebSockets.WebSocketState.Open)
                                                {
                                                    EtherealC.NativeClient.ClientCore.UnRegister($"NetNodeClient-{prefixes}", "ServerNetNodeService");
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                            //注册连接
                                            EtherealC.NativeClient.Client client = EtherealC.NativeClient.ClientCore.Register(serverNodeRequest, prefixes, clientConfig);
                                            client.ConnectEvent += Config_ConnectSuccessEvent;
                                            client.DisConnectEvent += Config_ConnectFailEvent;
                                            //部署
                                            net.Publish();
                                        }
                                        else throw new RPCException(RPCException.ErrorCode.Runtime, $"NetNode-Client-未找到Request:NetNodeClient-{prefixes}-ServerNetNodeService");
                                    }
                                    Thread.Sleep((int)cycle);
                                }
                            }
                            catch (Exception e)
                            {
                                OnException(e);
                            }
                        });
                        thread.Start(config.NetNodeHeartbeatCycle);
                    }
                    #endregion
                }
                server.Start();
            }
            catch(Exception e)
            {
                OnException(e);
            }
            return true;
        }

        internal void OnServerException(Exception exception, NativeServer.Server server)
        {
            OnException(exception);
        }

        internal void OnServerLog(RPCLog log, NativeServer.Server server)
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


        private void Config_ConnectSuccessEvent(EtherealC.NativeClient.Client client)
        {
            //注册节点信息
            if (EtherealC.RPCRequest.RequestCore.Get($"NetNodeClient-{client.Prefixes}", "ServerNetNodeService", out EtherealC.RPCRequest.Request serverDistributeRequest))
            {
                //生成节点信息
                NetNode node = new NetNode();
                node.Prefixes = new List<string>(server.Prefixes).ToArray();
                node.Name = $"{name}";
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
                    OnLog(RPCLog.LogCode.Runtime, $"分布式节点：{client.Prefixes}连接成功");
                }
            }
            else throw new RPCException(RPCException.ErrorCode.Runtime, $"EtherealC中未找到 NetNodeClient-{client.Prefixes}-ServerNodeService");
        }

        private void Config_ConnectFailEvent(EtherealC.NativeClient.Client client)
        {
            EtherealC.NativeClient.ClientCore.UnRegister(client.NetName,client.ServiceName);
        }

        public ClientResponseModel ClientRequestReceiveProcess(BaseToken token, ClientRequestModel request)
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
