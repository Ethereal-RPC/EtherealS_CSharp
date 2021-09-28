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
                return GetProxy(net, serviceName, out service);
            }
            else
            {
                service = null;
                return false;
            }
        }
        public static bool GetProxy(string netName,string serviceName, out Service.Abstract.Service service)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return GetProxy(net, serviceName, out service);
            }
            else
            {
                service = null;
                return false;
            }
        }
        public static bool GetProxy(Net.Abstract.Net net, string serviceName, out Service.Abstract.Service service)
        {
            return net.Services.TryGetValue(serviceName, out service);
        }
        public static T Register<T>(T instance, Net.Abstract.Net net, string servicename, AbstractTypes type) where T : Abstract.Service, new()
        {
            return Register<T>(instance,net, servicename, new ServiceConfig(type));
        }
        public static T Register<T>(Net.Abstract.Net net, string servicename, ServiceConfig config) where T : Abstract.Service, new()
        {
            return Register<T>(new T(), net, servicename, config);
        }
        public static T Register<T>(Net.Abstract.Net net, string servicename, AbstractTypes type) where T : Abstract.Service,new()
        {
            return Register<T>(new T(), net, servicename, new ServiceConfig(type));
        }
        public static T Register<T>(T instance, Net.Abstract.Net net, string servicename, ServiceConfig config) where T : Abstract.Service, new()
        {
            if (string.IsNullOrEmpty(servicename))
            {
                throw new ArgumentException("参数为空", nameof(servicename));
            }

            if (config.Types is null)
            {
                throw new ArgumentNullException(nameof(config.Types));
            }

            net.Services.TryGetValue(servicename, out Abstract.Service service);
            if (service == null)
            {
                Abstract.Service.Register(instance,net.Name,servicename,config);
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
