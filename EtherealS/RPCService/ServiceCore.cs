using EtherealS.Model;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace EtherealS.RPCService
{
    public class ServiceCore
    {
        public static bool Get(Tuple<string, string, string> key, out Service service)
        {
            if (NetCore.Get(new Tuple<string, string>(key.Item1, key.Item2), out Net net))
            {
                return net.Services.TryGetValue(key.Item3, out service);
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{key.Item1}-{key.Item2}Net未找到");
        }
        public static Service Register(object instance, string ip, string port, string servicename, RPCTypeConfig type)
        {
            return Register(instance,ip,port, servicename, new ServiceConfig(type));
        }
        public static Service Register<T>(string hostname, string port, string servicename, ServiceConfig config) where T : new()
        {
            return Register(new T(), hostname, port, servicename, config);
        }
        public static Service Register<T>(string hostname, string port, string servicename, RPCTypeConfig type) where T : new()
        {
            return Register(new T(), hostname, port, servicename, new ServiceConfig(type));
        }
        public static Service Register(object instance, string ip, string port, string servicename, ServiceConfig config)
        {
            if (string.IsNullOrEmpty(servicename))
            {
                throw new ArgumentException("参数为空", nameof(servicename));
            }

            if (string.IsNullOrEmpty(ip))
            {
                throw new ArgumentException("参数为空", nameof(ip));
            }

            if (string.IsNullOrEmpty(port))
            {
                throw new ArgumentException("参数为空", nameof(port));
            }

            if (config.Types is null)
            {
                throw new ArgumentNullException(nameof(config.Types));
            }
            if (!NetCore.Get(new Tuple<string, string>(ip, port), out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}Net未找到");
            }
            net.Services.TryGetValue(servicename, out Service service);
            if (service == null)
            {
                try
                {
                    service = new Service();
                    service.Register(new Tuple<string, string>(ip, port), servicename, instance, config);
                    net.Services[servicename] = service;
                    config.OnLog(RPCLog.LogCode.Register,$"{ip}-{port}-{servicename}注册成功！");
                    return service;
                }
                catch (Exception e)
                {
                    config.OnException(e);
                }
            }
            else config.OnException(new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}-{servicename}已注册！"));
            return null;
        }

        public static bool UnRegister(Tuple<string, string, string> key)
        {
            if (!NetCore.Get(new Tuple<string, string>(key.Item1, key.Item2), out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RegisterError, $"{key.Item1}-{key.Item2}Net未找到");
            }
            return net.Services.TryRemove(key.Item3, out Service value);
        }

        public static bool Get(string ip, string port, string servicename,out Service proxy)
        {
            if (!NetCore.Get(new Tuple<string, string>(ip, port), out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}Net未找到");
            }
            return net.Services.TryGetValue(servicename, out proxy);
        }
    }
}
