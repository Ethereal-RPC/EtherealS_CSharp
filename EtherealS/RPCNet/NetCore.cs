using EtherealS.Core.Model;
using EtherealS.NativeServer;
using EtherealS.RPCRequest;
using EtherealS.RPCService;
using System;
using System.Collections.Generic;
using EtherealS.RPCNet.Abstract;
using EtherealS.RPCNet.WebSocket;

namespace EtherealS.RPCNet
{
    /// <summary>
    /// 网关核心
    /// </summary>
    public class NetCore
    {
        public static Dictionary<string, Net> nets = new Dictionary<string, Net>();
        public static bool Get(string name, out Net net)
        {
            return nets.TryGetValue(name, out net);
        }

        public static Net Register(string name, Core.Enums.NetType netType)
        {
            return Register(name, new NetConfig(), netType);
        }
        public static Net Register(string name, NetConfig config,Core.Enums.NetType netType)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!nets.TryGetValue(name, out Net net))
            {
                if (netType == Core.Enums.NetType.WebSocket)
                {
                    net = new WebSocketNet();
                    net.Config = config;
                }
                else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{net.NetType}的Net-Register处理");
                net.Name = name;
                net.Config = config;
                nets.Add(name,net);
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{name} Net 已经注册");
            return net;
        }
        public static bool UnRegister(string name)
        {
            if (Get(name, out Net net))
            {
                return UnRegister(net);
            }
            return true;
        }
        public static bool UnRegister(Net net)
        {
            if(net != null)
            {
                foreach (string serviceName in net.Services.Keys)
                {
                    ServiceCore.UnRegister(net, serviceName);
                }
                foreach (string requestName in net.Requests.Keys)
                {
                    RequestCore.UnRegister(net, requestName);
                }
                ServerCore.UnRegister(net);
                nets.Remove(net.Name);
            }
            return true;
        }
    }
}
