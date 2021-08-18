using EtherealS.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using EtherealS.RPCService;
using System.Reflection;
using Newtonsoft.Json;

namespace EtherealS.RPCNet
{
    /// <summary>
    /// 网关核心
    /// </summary>
    public class NetCore
    {
        private static Dictionary<string, Net> nets { get; } = new Dictionary<string, Net>();

        public static bool Get(string name, out Net net)
        {
            return nets.TryGetValue(name,out net);
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
            else config.OnException(new RPCException(RPCException.ErrorCode.Core, $"{name} Net 已经注册"),null);
            return net;
        }
        public static bool UnRegister(string name)
        {
            if (Get(name, out Net net))
            {
                return UnRegister(net);
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{name} Net未找到");

        }
        public static bool UnRegister(Net net)
        {
            //**  这里应该执行一系列Net销毁操作
            return nets.Remove(net.Name);
        }
    }
}
