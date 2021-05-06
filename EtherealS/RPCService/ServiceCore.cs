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
        private static ConcurrentDictionary<Tuple<string, string, string>, Service> services { get; } = new ConcurrentDictionary<Tuple<string, string, string>, Service>();
        public static bool Get(Tuple<string, string, string> key, out Service service)
        {
            return services.TryGetValue(key, out service);
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

            Tuple<string, string, string> key = new Tuple<string, string, string>(ip, port,servicename);
            services.TryGetValue(key, out Service service);
            if (service == null)
            {
                try
                {
                    service = new Service();
                    service.Register(new Tuple<string, string>(ip, port), servicename, instance, config);
                    services[key] = service;
                    config.OnLog(RPCLog.LogCode.Register,$"{key.Item1}-{key.Item2}-{key.Item3}注册成功！");
                    return service;
                }
                catch (Exception e)
                {
                    config.OnException(e);
                }
            }
            else config.OnException(new RPCException(RPCException.ErrorCode.RegisterError, $"{key.Item1}-{key.Item2}-{key.Item3}已注册！"));
            return null;
        }

        public static void UnRegister(Tuple<string, string, string> key)
        {
            services.TryRemove(key, out Service value);
        }

        public static bool Get(string hostname, string port, string servicename,out Service proxy)
        {
            return services.TryGetValue(new Tuple<string, string, string>(hostname, port, servicename), out proxy);
        }
    }
}
