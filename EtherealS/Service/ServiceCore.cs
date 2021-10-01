using System;
using EtherealS.Core.Model;
using EtherealS.Net;
using EtherealS.Service.Abstract;
using EtherealS.Service.WebSocket;

namespace EtherealS.Service
{
    public class ServiceCore
    {
        public static bool Get<T>(string netName, string serviceName, out Abstract.Service service)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get(net, serviceName, out service);
            }
            else
            {
                service = null;
                return false;
            }
        }
        public static bool Get(string netName,string serviceName, out Service.Abstract.Service service)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get(net, serviceName, out service);
            }
            else
            {
                service = null;
                return false;
            }
        }
        public static bool Get(Net.Abstract.Net net, string serviceName, out Abstract.Service service)
        {
            return net.Services.TryGetValue(serviceName, out service);
        }

        public static T Register<T>(Net.Abstract.Net net, T service) where T : Abstract.Service
        {
            return Register(net, service, null, null);
        }
        public static T Register<T>(Net.Abstract.Net net, T service, string serviceName, AbstractTypes types) where T: Abstract.Service
        {
            if (serviceName != null) service.Name = serviceName;
            if (types != null) service.Types = types;
            Abstract.Service.Register(service);
            if (!net.Services.ContainsKey(service.Name))
            {
                service.NetName = net.Name;
                service.LogEvent += net.OnLog;
                service.ExceptionEvent += net.OnException;
                net.Services[service.Name] = service;
                return service;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}-{service.Name}已注册！");
        }

        public static bool UnRegister(string netName,string serviceName)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return UnRegister(net, serviceName);
            }
            return true;
        }
        public static bool UnRegister(Net.Abstract.Net net, string serviceName)
        {
            if(net != null)
            {
                net.Services.TryRemove(serviceName, out Service.Abstract.Service service);
                if(service != null)
                {
                    service.LogEvent -= net.OnLog;
                    service.ExceptionEvent -= net.OnException;
                }
            }
            return true;
        }
    }
}
