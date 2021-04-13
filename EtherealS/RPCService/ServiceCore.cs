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
        public static void Register(object instance, string servicename, string ip, string port,RPCType type)
        {
            Register(instance,servicename,ip,port,new ServiceConfig(type));
        }
        public static void Register(object instance,string servicename, string ip, string port, ServiceConfig config)
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

            if (config.Type is null)
            {
                throw new ArgumentNullException(nameof(config.Type));
            }

            Tuple<string, string, string> key = new Tuple<string, string, string>(servicename, ip, port);
            services.TryGetValue(key, out Service service);
            if (service == null)
            {
                try
                {
                    service = new Service();
                    service.Register(key,instance,config);
                    services[key] = service;
                    Console.WriteLine($"{servicename}-{ip}-{port} Load Success!");
                }
                catch (RPCException e)
                {
                    Console.WriteLine($"{servicename}-{ip}-{port} Load Fail!");
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                    UnRegister(key);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{servicename}-{ip}-{port} Load Fail!");
                    Console.WriteLine("发生异常报错,销毁注册");
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                    UnRegister(key);
                }
            }
        }
        public static void Register<T>(string servicename, string hostname, string port, ServiceConfig config) where T : new()
        {
            Register(new T(), servicename, hostname, port, config);
        }
        public static void Register<T>(string servicename, string hostname, string port,RPCType type) where T : new()
        {
            Register(new T(), servicename, hostname, port, new ServiceConfig(type));
        }
        public static void UnRegister(Tuple<string, string, string> key)
        {
            services.TryRemove(key, out Service value);
        }

        public static bool Get(string servicename, string hostname, string port, out Service proxy)
        {
            return services.TryGetValue(new Tuple<string, string, string>(servicename, hostname, port), out proxy);
        }
    }
}
