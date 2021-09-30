using System;
using System.Collections.Generic;
using EtherealS.Core.Model;
using EtherealS.Net.Abstract;
using EtherealS.Net.WebSocket;
using EtherealS.Request;
using EtherealS.Server;
using EtherealS.Service;

namespace EtherealS.Net
{
    /// <summary>
    /// 网关核心
    /// </summary>
    public class NetCore
    {
        public static Dictionary<string, Net.Abstract.Net> nets = new Dictionary<string, Net.Abstract.Net>();
        public static bool Get(string name, out Net.Abstract.Net net)
        {
            return nets.TryGetValue(name, out net);
        }
        
        public static Abstract.Net Register(Abstract.Net net)
        {
            if (!nets.ContainsKey(net.Name))
            {
                nets.Add(net.Name,net);
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name} Net 已经注册");
            return net;
        }
        public static bool UnRegister(string name)
        {
            if (Get(name, out Net.Abstract.Net net))
            {
                return UnRegister(net);
            }
            return true;
        }
        public static bool UnRegister(Net.Abstract.Net net)
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
