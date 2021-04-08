using System;
using System.Collections.Generic;
using EtherealS.Model;

namespace EtherealS.RPCRequest
{
    public class RequestCore
    {
        private static Dictionary<Tuple<string, string, string>, Request> requests { get; } = new Dictionary<Tuple<string, string, string>, Request>();
        public static bool Get(string requestname,string ip,string port, out Request reqeust)
        {
            return Get(new Tuple<string, string, string>(requestname, ip, port),out reqeust);
        }
        public static bool Get(Tuple<string, string, string> key, out Request reqeust)
        {
            return requests.TryGetValue(key, out reqeust);
        }
        public static R Register<R>(string servicename, string ip, string port,RPCType type)
        {
            if (type is null)
            {
                throw new ArgumentException("参数为空", nameof(type));
            }
            RequestConfig config = new RequestConfig(type);
            return Register<R>(servicename, ip, port, config);
        }

        public static R Register<R>(string servicename,string ip, string port,RequestConfig config)
        {
            if (string.IsNullOrEmpty(servicename))
            {
                Console.WriteLine($"{servicename}-{ip}-{port} Load Fail!");
                throw new ArgumentException("参数为空", nameof(servicename));
            }

            if (string.IsNullOrEmpty(ip))
            {
                Console.WriteLine($"{servicename}-{ip}-{port} Load Fail!");
                throw new ArgumentException("参数为空", nameof(ip));
            }

            if (string.IsNullOrEmpty(port))
            {
                Console.WriteLine($"{servicename}-{ip}-{port} Load Fail!");
                throw new ArgumentException("参数为空", nameof(port));
            }

            if (config.Type is null)
            {
                throw new ArgumentNullException(nameof(config.Type));
            }
            Tuple<string, string, string> key = new Tuple<string, string, string>(servicename, ip, port);
            requests.TryGetValue(key,out Request request);
            if (request == null)
            {
                request = Request.Register<R>(servicename, new Tuple<string, string>(ip, port), config);
                requests[key] = request;
            }
            else throw new RPCException(RPCException.ErrorCode.RegisterError, $"{key}已注册，无法重复注册！");
            return (R)(request as object);
        }
        public static void UnRegister(Tuple<string, string, string> key)
        {
            requests.Remove(key, out Request value);
        }
    }
}
