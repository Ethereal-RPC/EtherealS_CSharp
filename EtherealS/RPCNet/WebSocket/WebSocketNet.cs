using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using EtherealS.Core.Model;
using EtherealS.RPCNet.Abstract;
using EtherealS.RPCNet.NetNode.Model;
using EtherealS.RPCNet.NetNode.NetNodeClient.Request;
using EtherealS.RPCNet.NetNode.NetNodeClient.Service;
using EtherealS.RPCNet.NetNode.NetNodeServer.Request;
using EtherealS.RPCNet.NetNode.NetNodeServer.Service;
using EtherealS.RPCRequest;
using EtherealS.RPCRequest.Abstract;
using EtherealS.RPCService;
using EtherealS.RPCService.Abstract;

namespace EtherealS.RPCNet.WebSocket
{
    public class WebSocketNet:Net
    {
        #region --属性--
        public new WebSocketNetConfig Config { get => (WebSocketNetConfig)config; set => config = value; }

        #endregion

        #region --方法--
        /// <summary>
        /// 部署节点
        /// </summary>
        /// <returns></returns>
        public override bool Publish()
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
                        types.Add<NetNode.Model.NetNode>("NetNode");
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
                            EtherealC.Core.Model.RPCTypeConfig types = new EtherealC.Core.Model.RPCTypeConfig();
                            types.Add<int>("Int");
                            types.Add<long>("Long");
                            types.Add<string>("String");
                            types.Add<bool>("Bool");
                            types.Add<NetNode.Model.NetNode>("NetNode");
                            EtherealC.RPCNet.Net net = EtherealC.RPCNet.NetCore.Register($"NetNodeClient-{prefixes}", EtherealC.Core.Enums.NetType.WebSocket);
                            net.Config.NetNodeMode = false;
                            //注册服务
                            ClientNodeService clientNodeService = (ClientNodeService)EtherealC.RPCService.ServiceCore.Register<ClientNodeService>(net, "ClientNetNodeService", types).Instance;
                            //注册请求
                            clientNodeService.ServerNodeRequest = EtherealC.RPCRequest.RequestCore.Register<ServerNodeRequest>(net, "ServerNetNodeService", types);
                            net.LogEvent += Net_LogEvent; ;
                            net.ExceptionEvent += OnException;
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
                                                continue;
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

        private void Net_LogEvent(EtherealC.Core.Model.RPCLog log)
        {
            OnLog(RPCLog.LogCode.Runtime,"NetNodeClient::" + log.Message);
        }

        private void Config_ConnectSuccessEvent(EtherealC.NativeClient.Client client)
        {
            //注册节点信息
            if (EtherealC.RPCRequest.RequestCore.Get($"NetNodeClient-{(client as EtherealC.NativeClient.WebSocketClient).Prefixes}", "ServerNetNodeService", out EtherealC.RPCRequest.Request serverDistributeRequest))
            {
                //生成节点信息
                NetNode.Model.NetNode node = new NetNode.Model.NetNode();
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
                    OnLog(RPCLog.LogCode.Runtime, $"分布式节点：{(client as EtherealC.NativeClient.WebSocketClient).Prefixes}连接成功");
                }
            }
            else throw new RPCException(RPCException.ErrorCode.Runtime, $"EtherealC中未找到 NetNodeClient-{(client as EtherealC.NativeClient.WebSocketClient).Prefixes}-ServerNodeService");
        }

        private void Config_ConnectFailEvent(EtherealC.NativeClient.Client client)
        {
            EtherealC.NativeClient.ClientCore.UnRegister(client.NetName,client.ServiceName);
        }

        #endregion
    }
}
