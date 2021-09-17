using System;
using EtherealS.Core.Model;
using EtherealS.Net;
using EtherealS.Service.Abstract;
using EtherealS.Service.WebSocket;

namespace EtherealS.Service
{
    public class ServiceCore
    {
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
        public static bool Get(Net.Abstract.Net net, string serviceName, out Service.Abstract.Service service)
        {
            return net.Services.TryGetValue(serviceName, out service);
        }
        public static Service.Abstract.Service Register(object instance, Net.Abstract.Net net, string servicename, AbstractTypes type)
        {
            return Register(instance,net, servicename, new ServiceConfig(type));
        }
        public static Service.Abstract.Service Register<T>(Net.Abstract.Net net, string servicename, ServiceConfig config) where T : new()
        {
            return Register(new T(), net, servicename, config);
        }
        public static Service.Abstract.Service Register<T>(Net.Abstract.Net net, string servicename, AbstractTypes type) where T : new()
        {
            return Register(new T(), net, servicename, new ServiceConfig(type));
        }
        public static Service.Abstract.Service Register(object instance, Net.Abstract.Net net, string servicename, ServiceConfig config)
        {
            if (string.IsNullOrEmpty(servicename))
            {
                throw new ArgumentException("参数为空", nameof(servicename));
            }

            if (config.Types is null)
            {
                throw new ArgumentNullException(nameof(config.Types));
            }

            net.Services.TryGetValue(servicename, out Service.Abstract.Service service);
            if (service == null)
            {
                if (net.Type == Net.Abstract.Net.NetType.WebSocket) 
                {    
                    service = new WebSocketService();
                }
                else throw new TrackException(TrackException.ErrorCode.Core, $"未有针对{net.Type}的Service-Register处理");
                service.Register(net.Name, servicename, instance, config);
                net.Services[servicename] = service;
                service.LogEvent += net.OnLog;
                service.ExceptionEvent += net.OnException;
                return service;
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
