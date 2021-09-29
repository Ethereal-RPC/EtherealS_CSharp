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
        public static T Register<T>(Net.Abstract.Net net, string servicename, AbstractTypes types, ServiceConfig config = null) where T : Abstract.Service,new()
        {
            return Register<T>(new T(), net, servicename,types, config);
        }
        public static T Register<T>(T instance, Net.Abstract.Net net, string servicename, AbstractTypes types, ServiceConfig config = null) where T : Abstract.Service, new()
        {
            net.Services.TryGetValue(servicename, out Abstract.Service service);
            if (service == null)
            {
                Abstract.Service.Register(instance,net.Name,servicename,types,config);
                net.Services[servicename] = instance;
                instance.LogEvent += net.OnLog;
                instance.ExceptionEvent += net.OnException;
                return instance;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}-{servicename}已注册！");
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
