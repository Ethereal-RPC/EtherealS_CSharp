﻿using EtherealS.Core.Model;
using EtherealS.Net;

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
        public static bool Get(string netName, string serviceName, out Service.Abstract.Service service)
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

        public static T Register<T>(Net.Abstract.Net net, T service, string serviceName = null) where T : Abstract.Service
        {
            service.Initialize();
            if (serviceName != null) service.name = serviceName;
            if (!service.IsRegister)
            {
                service.isRegister = true;
                service.Net = net;
                service.LogEvent += net.OnLog;
                service.ExceptionEvent += net.OnException;
                net.Services[service.Name] = service;
                Abstract.Service.Register(service);
                service.Register();
                return service;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{net.Name}-{service.Name}已注册！");
        }
        public static bool UnRegister(Abstract.Service service)
        {
            if (service.IsRegister)
            {
                service.UnRegister();
                if(service.Net != null)
                {
                    service.Net.Services.TryRemove(service.Name, out service);
                    service.LogEvent -= service.Net.OnLog;
                    service.ExceptionEvent -= service.Net.OnException;
                    service.Net = null;
                }
                service.UnInitialize();
                service.isRegister = false;
                return true;
            }
            else throw new TrackException(TrackException.ErrorCode.Core, $"{service.Name}并未注册，无需UnRegister");
        }
    }
}
