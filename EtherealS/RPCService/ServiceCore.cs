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
        public static bool Get(Tuple<string, string, string> key, out Service config)
        {
            return services.TryGetValue(key, out config);
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
                    service.Register(instance,config);
                    services[key] = service;
                    Console.WriteLine($"{servicename}-{ip}-{port} Load Success!");
                }
                catch (RPCException e)
                {
                    Console.WriteLine($"{servicename}-{ip}-{port} Load Fail!");
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                    UnRegister(servicename, ip, port);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{servicename}-{ip}-{port} Load Fail!");
                    Console.WriteLine("发生异常报错,销毁注册");
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                    UnRegister(servicename, ip, port);
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
        public static void UnRegister(string servicename, string hostname, string port)
        {
            services.TryRemove(new Tuple<string, string, string>(servicename,hostname,port), out Service value);
        }

        public static bool Get(string servicename, string hostname, string port, out Service proxy)
        {
            return services.TryGetValue(new Tuple<string, string, string>(servicename, hostname, port), out proxy);
        }
        public static void ClientRequestReceive(Tuple<string,string> key,BaseUserToken token,ClientRequestModel request)
        {
#if DEBUG
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"{DateTime.Now}::{key.Item1}:{key.Item2}::[客-请求]\n{request}");
            Console.WriteLine("--------------------------------------------------");
#endif
            if (services.TryGetValue(new Tuple<string, string, string>(key.Item1, key.Item2, request.Service), out Service service))
            {
                if (service.Methods.TryGetValue(request.MethodId, out MethodInfo method))
                {
                    if (NetCore.Get(key, out NetConfig netConfig))
                    {
                        if (netConfig.OnInterceptor(service, method, token) &&
                            service.Config.OnInterceptor(service, method, token))
                        {
                            if (service.Config.TokenEnable && request.Params.Length >= 1) request.Params[0] = token;
                            object result = method.Invoke(service.Instance, request.Params);
                            service.Config.Type.AbstractName.TryGetValue(method.ReturnType, out string type);
                            netConfig.ClientResponseSend(token, new ClientResponseModel("2.0", JsonConvert.SerializeObject(result), type, request.Id, request.Service, null));
                        }
                    }
                    else throw new RPCException(RPCException.ErrorCode.NotFoundNetConfig, $"未找到NetConfig[{key.Item1}:{key.Item2}]");
                }
                else throw new RPCException(RPCException.ErrorCode.NotFoundMethod, $"未找到方法[{request.MethodId}]");
            }
            else throw new RPCException(RPCException.ErrorCode.NotFoundService, $"未找到服务[{key.Item1}:{key.Item2}::{request.Service}]");
        }
        public static void ClientRequestReceiveVoid(Tuple<string, string> key, BaseUserToken token, ClientRequestModel request)
        {
#if DEBUG
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"{DateTime.Now}::{key.Item1}:{key.Item2}::[客-请求]\n{request}");
            Console.WriteLine("--------------------------------------------------");
#endif
            if (services.TryGetValue(new Tuple<string, string, string>(key.Item1, key.Item2, request.Service), out Service service))
            {
                if (service.Methods.TryGetValue(request.MethodId, out MethodInfo method))
                {
                    if (NetCore.Get(key, out NetConfig netConfig))
                    {
                        if (netConfig.OnInterceptor(service, method, token) &&
                            service.Config.OnInterceptor(service, method, token))
                        {
                            if (service.Config.TokenEnable && request.Params.Length >= 1) request.Params[0] = token;
                            method.Invoke(service.Instance, request.Params);
                        }
                    }
                    else throw new RPCException(RPCException.ErrorCode.NotFoundNetConfig, $"未找到NetConfig[{key.Item1}:{key.Item2}]");
                }
                else throw new RPCException(RPCException.ErrorCode.NotFoundMethod, $"未找到方法[{request.MethodId}]");
            }
            else throw new RPCException(RPCException.ErrorCode.NotFoundService, $"未找到服务[{key.Item1}:{key.Item2}::{request.Service}]");
        }
    }
}
