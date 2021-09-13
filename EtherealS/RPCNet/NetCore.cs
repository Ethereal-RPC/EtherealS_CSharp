using EtherealS.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using EtherealS.RPCService;
using System.Reflection;
using Newtonsoft.Json;
using EtherealS.RPCRequest;
using EtherealS.NativeServer;

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

        public static Net Register(string name)
        {
            return Register(name, new NetConfig());
        }
        public static Net Register(string name, NetConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!nets.TryGetValue(name, out Net net))
            {
                net = new Net();
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
