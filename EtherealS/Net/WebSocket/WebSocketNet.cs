using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using EtherealS.Core.Model;
using EtherealS.Net.NetNode.Model;
using EtherealS.Net.NetNode.NetNodeClient.Request;
using EtherealS.Net.NetNode.NetNodeClient.Service;
using EtherealS.Net.NetNode.NetNodeServer.Request;
using EtherealS.Net.NetNode.NetNodeServer.Service;
using EtherealS.Request;
using EtherealS.Service;

namespace EtherealS.Net.WebSocket
{
    public class WebSocketNet:Abstract.Net
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
                        AbstractTypes types = new AbstractTypes();
                        types.Add<int>("Int");
                        types.Add<long>("Long");
                        types.Add<string>("String");
                        types.Add<bool>("Bool");
                        types.Add<NetNode.Model.NetNode>("NetNode");
                        //注册服务
                        ServerNodeService serverDistributeService = ServiceCore.Register<ServerNodeService>(this, "ServerNetNodeService", types);
                        //注册请求
                        serverDistributeService.DistributeRequest = RequestCore.Register<ClientNodeRequest, IClientNodeRequest>(this, "ClientNetNodeService", types);
                    }
                    #endregion

                    #region --Client--
                    foreach (Tuple<string, EtherealC.Client.Abstract.ClientConfig> item in config.NetNodeIps)
                    {
                        string prefixes = item.Item1;
                        EtherealC.Client.Abstract.ClientConfig clientConfig = item.Item2;
                        //tip:S项目引入C，对于C的引用，命名必须全部采取全名引用，避免冲突。
                        //注册数据类型
                        EtherealC.Core.Model.AbstractTypes types = new EtherealC.Core.Model.AbstractTypes();
                        types.Add<int>("Int");
                        types.Add<long>("Long");
                        types.Add<string>("String");
                        types.Add<bool>("Bool");
                        types.Add<NetNode.Model.NetNode>("NetNode");
                        EtherealC.Net.Abstract.Net net = EtherealC.Net.NetCore.Register($"NetNodeClient-{prefixes}", EtherealC.Net.Abstract.Net.NetType.WebSocket);
                        net.Config.NetNodeMode = false; 
                        //注册服务
                        ClientNodeService clientNodeService = (ClientNodeService)EtherealC.Service.ServiceCore.Register<ClientNodeService>(net, "ClientNetNodeService", types).Instance;
                        //注册请求
                        clientNodeService.ServerNodeRequest = EtherealC.Request.RequestCore.Register<ServerNodeRequest>(net, "ServerNetNodeService", types);
                        net.LogEvent += Net_LogEvent;
                        net.ExceptionEvent += Net_ExceptionEvent;
                    }
                    Thread thread = new Thread((cycle) => {
                        while (true)
                        {
                            try
                            {
                                foreach (Tuple<string, EtherealC.Client.Abstract.ClientConfig> item in config.NetNodeIps)
                                {
                                    string prefixes = item.Item1;
                                    EtherealC.Client.Abstract.ClientConfig clientConfig = item.Item2;
                                    if (!EtherealC.Net.NetCore.Get($"NetNodeClient-{prefixes}", out EtherealC.Net.Abstract.Net net))
                                    {
                                        throw new TrackException(TrackException.ErrorCode.Runtime, $"NetNode-Client-未找到Net:NetNodeClient-{prefixes}");
                                    }
                                    if (EtherealC.Request.RequestCore.Get(net, "ServerNetNodeService", out EtherealC.Request.Abstract.Request serverNodeRequest))
                                    {
                                        if (serverNodeRequest.Client != null)
                                        {
                                            continue;
                                        }
                                        //注册连接
                                        EtherealC.Client.Abstract.Client client = EtherealC.Client.ClientCore.Register(serverNodeRequest, prefixes, clientConfig);
                                        client.ConnectEvent += Config_ConnectSuccessEvent;
                                        client.DisConnectEvent += Config_ConnectFailEvent;
                                        //部署
                                        net.Publish();
                                    }
                                    else throw new TrackException(TrackException.ErrorCode.Runtime, $"NetNode-Client-未找到Request:NetNodeClient-{prefixes}-ServerNetNodeService");
                                }
                            }
                            catch (TrackException e)
                            {
                                OnException(e);
                            }
                            finally
                            {
                                Thread.Sleep((int)cycle);
                            }
                        }

                    });
                    thread.Start(config.NetNodeHeartbeatCycle);
                    #endregion
                }
                server.Start();
            }
            catch(TrackException e)
            {
                OnException(e);
            }
            return true;
        }

        private void Net_LogEvent(EtherealC.Core.Model.TrackLog log)
        {
            OnLog(TrackLog.LogCode.Runtime,"NetNodeClient::" + log.Message);
        }
        private void Net_ExceptionEvent(EtherealC.Core.Model.TrackException exception)
        {
            OnException(new TrackException(exception));
        }

        private void Config_ConnectSuccessEvent(EtherealC.Client.Abstract.Client client)
        {
            //注册节点信息
            if (EtherealC.Request.RequestCore.Get($"NetNodeClient-{(client as EtherealC.Client.WebSocket.WebSocketClient).Prefixes}", "ServerNetNodeService", out EtherealC.Request.Abstract.Request serverDistributeRequest))
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
                ((ServerNodeRequest)serverDistributeRequest).Register(node);
                //向目标主机注册节点信息
                if (true)
                {
                    OnLog(TrackLog.LogCode.Runtime, $"分布式节点：{(client as EtherealC.Client.WebSocket.WebSocketClient).Prefixes}连接成功");
                }
            }
            else throw new TrackException(TrackException.ErrorCode.Runtime, $"EtherealC中未找到 NetNodeClient-{(client as EtherealC.Client.WebSocket.WebSocketClient).Prefixes}-ServerNodeService");
        }

        private void Config_ConnectFailEvent(EtherealC.Client.Abstract.Client client)
        {
            EtherealC.Client.ClientCore.UnRegister(client.NetName,client.ServiceName);
        }

        #endregion
    }
}
