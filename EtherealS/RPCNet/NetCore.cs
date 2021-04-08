using EtherealS.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using EtherealS.RPCService;

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

        public static void Register(string ip, string port,NetConfig.CreateInstance createMethod)
        {
            if (createMethod is null)
            {
                throw new ArgumentNullException(nameof(createMethod));
            }
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
                configs.Add(new Tuple<string, string>(ip, port),config);
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{ip}-{port}服务的NetConfig已经注册");
        }
        public static bool UnRegister(string ip, string port)
        {
            return configs.Remove(new Tuple<string, string>(ip, port));
        }
    }
}
