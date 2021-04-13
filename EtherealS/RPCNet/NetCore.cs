using EtherealS.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using EtherealS.RPCService;
using System.Reflection;
using Newtonsoft.Json;

namespace EtherealS.RPCNet
{
    /// <summary>
    /// 客户端工厂
    /// </summary>
    public class NetCore
    {
        private static Dictionary<Tuple<string, string>, NetConfig> configs { get; } = new Dictionary<Tuple<string, string>, NetConfig>();

        public static bool Get(Tuple<string, string> key, out NetConfig config)
        {
            return configs.TryGetValue(key,out config);
        }

        public static ConcurrentDictionary<object, BaseUserToken> GetTokens(Tuple<string, string> key)
        {
            if (configs.TryGetValue(key, out NetConfig config))
            {
                return config.Tokens;
            }
            return null;
        }
        public static void Register(string ip, string port)
        {
            Register(ip,port,new NetConfig());
        }
        public static void Register(string ip, string port, NetConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!configs.TryGetValue(new Tuple<string, string>(ip, port), out NetConfig value))
            {
                if (config.ClientRequestReceive == null) config.ClientRequestReceive = ClientRequestReceive;
                configs.Add(new Tuple<string, string>(ip, port),config);
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}服务的NetConfig已经注册");
        }
        public static bool UnRegister(string ip, string port)
        {
            return configs.Remove(new Tuple<string, string>(ip, port));
        }
        private static void ClientRequestReceive(Tuple<string, string> key, BaseUserToken token, ClientRequestModel request)
        {
#if DEBUG
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"{DateTime.Now}::{key.Item1}:{key.Item2}::[客-请求]\n{request}");
            Console.WriteLine("--------------------------------------------------");
#endif
            if (ServiceCore.Get(new Tuple<string, string, string>(request.Service,key.Item1, key.Item2), out Service service))
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
                            Type return_type = method.ReturnType;
                            if (return_type != typeof(void))
                            {
                                service.Config.Type.AbstractName.TryGetValue(return_type, out string type);
                                netConfig.ClientResponseSend(token, new ClientResponseModel("2.0", JsonConvert.SerializeObject(result), type, request.Id, request.Service, null));
                            }
                        }
                    }
                    else throw new RPCException(RPCException.ErrorCode.RegisterError, $"未找到NetConfig[{key.Item1}:{key.Item2}]");
                }
                else throw new RPCException(RPCException.ErrorCode.RegisterError, $"未找到方法[{key.Item1}:{key.Item2}:{request.Service}:{request.MethodId}]");
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"未找到服务[{key.Item1}:{key.Item2}:{request.Service}]");
        }
    }
}
