using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using EtherealS.Core.Model;
using EtherealS.Net.Extension.NetNode.Model;
using EtherealS.Net.Extension.NetNode.NetNodeClient.Request;
using EtherealS.Net.Extension.NetNode.NetNodeClient.Service;
using EtherealS.Net.Extension.NetNode.NetNodeServer.Request;
using EtherealS.Net.Extension.NetNode.NetNodeServer.Service;
using EtherealS.Request;
using EtherealS.Service;

namespace EtherealS.Net.WebSocket
{
    public class WebSocketNet:Abstract.Net
    {
        #region --属性--
        public new WebSocketNetConfig Config { get => (WebSocketNetConfig)config; set => config = value; }
        public AutoResetEvent sign = new AutoResetEvent(false);
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
                if (config.NetNodeMode) //开启分布式模式
                {
                    #region --Server--
                    {
                        //注册数据类型
                        AbstractTypes types = new AbstractTypes();
                        types.Add<int>("Int");
                        types.Add<long>("Long");
                        types.Add<string>("String");
                        types.Add<bool>("Bool");
                        types.Add<NetNode>("NetNode");
                        //注册服务
                        ServerNodeService serverDistributeService = ServiceCore.Register(this, new ServerNodeService(),
                            "ServerNetNodeService", types);
                        //注册请求
                        serverDistributeService.DistributeRequest =
                            RequestCore.Register<ClientNodeRequest, IClientNodeRequest>(this, "ClientNetNodeService",
                                types);
                    }
                    #endregion

                    #region --Client--
                    Thread thread = new Thread((cycle) =>
                    {
                        while (NetCore.Get(name, out Abstract.Net temp))
                        {
                            try
                            {
                                foreach (Tuple<string, EtherealC.Client.Abstract.ClientConfig> item in
                                    config.NetNodeIps)
                                {
                                    string prefixes = item.Item1;
                                    EtherealC.Client.Abstract.ClientConfig clientConfig = item.Item2;
                                    if (!EtherealC.Net.NetCore.Get($"NetNodeClient-{prefixes}",
                                        out EtherealC.Net.Abstract.Net net))
                                    {
                                        net = EtherealC.Net.NetCore.Register(new EtherealC.Net.WebSocket.WebSocketNet($"NetNodeClient-{prefixes}"));
                                        net.Config.NetNodeMode = false;
                                        net.LogEvent += Net_LogEvent;
                                        net.ExceptionEvent += Net_ExceptionEvent;
                                    }

                                    if (!EtherealC.Request.RequestCore.Get(net, $"ServerNetNodeService", out ServerNodeRequest serverNodeRequest))
                                    {
                                        //注册请求
                                        serverNodeRequest = EtherealC.Request.RequestCore.Register<ServerNodeRequest, IServerNodeRequest>(net);
                                    }

                                    if (!EtherealC.Service.ServiceCore.Get(net, "ClientNetNodeService", out ClientNodeService clientNodeService))
                                    {
                                        //注册服务
                                        clientNodeService = EtherealC.Service.ServiceCore.Register(net, new ClientNodeService());
                                    }
                                    if (serverNodeRequest.Client == null)
                                    {
                                        EtherealC.Client.Abstract.Client client =
                                            new EtherealC.Client.WebSocket.WebSocketClient(prefixes);
                                        client.Config = clientConfig;
                                        //注册连接
                                        EtherealC.Client.ClientCore.Register(serverNodeRequest, client);
                                        client.ConnectEvent += ClientConnectSuccessEvent;
                                        client.ConnectFailEvent += ClientConnectFailEvent;
                                        client.DisConnectEvent += ClientDisConnectEvent;
                                        //部署
                                        net.Publish();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                OnException(new TrackException(e));
                            }
                            finally
                            {
                                sign.WaitOne(config.NetNodeHeartbeatCycle);
                            }
                        }
                    });
                    thread.Start(config.NetNodeHeartbeatCycle);

                    #endregion
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



        private void Net_LogEvent(EtherealC.Core.Model.TrackLog log)
        {
            OnLog(TrackLog.LogCode.Runtime,"NetNodeClient\n" + log.Message);
        }
        private void Net_ExceptionEvent(EtherealC.Core.Model.TrackException exception)
        {
            OnException(new TrackException(exception));
        }

        private void ClientConnectSuccessEvent(EtherealC.Client.Abstract.Client client)
        {
            //注册节点信息
            if (EtherealC.Request.RequestCore.Get($"NetNodeClient-{client.Prefixes}", "ServerNetNodeService", out EtherealC.Request.Abstract.Request serverDistributeRequest))
            {
                //生成节点信息
                NetNode node = new NetNode();
                node.Prefixes = server.Prefixes.ToArray();
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
                foreach (Service.Abstract.Service service in services.Values)
                {
                    ServiceNode serviceNode = new ServiceNode();
                    serviceNode.Name = service.Name;
                    node.Services.Add(serviceNode.Name, serviceNode);
                }
                //添加请求信息    
                foreach (Request.Abstract.Request request in requests.Values)
                {
                    RequestNode requestNode = new RequestNode();
                    requestNode.Name = request.Name;
                    node.Requests.Add(requestNode.Name, requestNode);
                }
                //向目标主机注册节点信息
                ((IServerNodeRequest)serverDistributeRequest).Register(node);
            }
            else throw new TrackException(TrackException.ErrorCode.Runtime, $"EtherealC中未找到 NetNodeClient-{client.Prefixes}-ServerNodeService");
        }

        private void ClientConnectFailEvent(EtherealC.Client.Abstract.Client client)
        {
            EtherealC.Client.ClientCore.UnRegister(client.NetName,client.ServiceName);
        }
        private void ClientDisConnectEvent(EtherealC.Client.Abstract.Client client)
        {
            EtherealC.Client.ClientCore.UnRegister(client.NetName, client.ServiceName);
            sign.Set();
        }
        #endregion

        public WebSocketNet(string name) : base(name)
        {
            this.config = new WebSocketNetConfig();
            this.Type = NetType.WebSocket;
        }
    }
}
