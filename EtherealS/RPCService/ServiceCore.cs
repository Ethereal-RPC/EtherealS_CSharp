﻿using EtherealS.Model;
using EtherealS.RPCNet;
using System;

namespace EtherealS.RPCService
{
    public class ServiceCore
    {
        public static bool Get(string netName,string serviceName, out Service service)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return Get(net, serviceName, out service);
            }
            else
            {
                service = null;
                return false;
            }
        }
        public static bool Get(Net net, string serviceName, out Service service)
        {
            return net.Services.TryGetValue(serviceName, out service);
        }
        public static Service Register(object instance, Net net, string servicename, RPCTypeConfig type)
        {
            return Register(instance,net, servicename, new ServiceConfig(type));
        }
        public static Service Register<T>(Net net, string servicename, ServiceConfig config) where T : new()
        {
            return Register(new T(), net, servicename, config);
        }
        public static Service Register<T>(Net net, string servicename, RPCTypeConfig type) where T : new()
        {
            return Register(new T(), net, servicename, new ServiceConfig(type));
        }
        public static Service Register(object instance, Net net, string servicename, ServiceConfig config)
        {
            if (string.IsNullOrEmpty(servicename))
            {
                throw new ArgumentException("参数为空", nameof(servicename));
            }

            if (config.Types is null)
            {
                throw new ArgumentNullException(nameof(config.Types));
            }

            net.Services.TryGetValue(servicename, out Service service);
            if (service == null)
            {
                if(service is NetNodeService)
                {
                    service = new NetNodeService();
                    service.Register(net.Name, servicename, instance, config);
                }
                net.Services[servicename] = service;
                service.LogEvent += net.OnLog;
                service.ExceptionEvent += net.OnException;
                return service;
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{net.Name}-{servicename}已注册！");
        }

        public static bool UnRegister(string netName,string serviceName)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return UnRegister(net, serviceName);
            }
            return true;
        }
        public static bool UnRegister(Net net, string serviceName)
        {
            if(net != null)
            {
                net.Services.TryRemove(serviceName, out Service service);
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
