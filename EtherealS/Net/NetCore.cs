using EtherealS.Core.Model;
using EtherealS.Request;
using EtherealS.Server;
using EtherealS.Service;
using System.Collections.Generic;

namespace EtherealS.Net
{
    /// <summary>
    /// 网关核心
    /// </summary>
    public class NetCore
    {
        public static Dictionary<string, Abstract.Net> nets = new Dictionary<string, Abstract.Net>();

        public static bool Get(string name, out Abstract.Net net)
        {
            return nets.TryGetValue(name, out net);
        }

        public static Abstract.Net Register(Abstract.Net net)
        {
            if (!net.IsRegister)
            {
                net.isRegister = true;
                nets.Add(net.Name, net);
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name} Net 已经注册");
            return net;
        }
        public static bool UnRegister(string name)
        {
            if (Get(name, out Abstract.Net net))
            {
                return UnRegister(net);
            }
            return true;
        }
        public static bool UnRegister(Abstract.Net net)
        {
            if (net.IsRegister)
            {
                nets.Remove(net.Name);
                net.isRegister = false;
                return true;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}并未注册，无需UnRegister");
        }
    }
}
