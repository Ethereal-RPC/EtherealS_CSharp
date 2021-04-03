using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using EtherealS.Model;
using EtherealS.RPCNet;

namespace EtherealS.NativeNetwork
{
    public class AsyncServerCore
    {
        private static Dictionary<Tuple<string, string>, SocketListener> socketservers { get; } = new Dictionary<Tuple<string, string>, SocketListener>();

        public static SocketListener Register(string ip, string port,BaseUserToken.CreateInstance createMethod)
        {
            return Register(ip, port, new ServerConfig(createMethod),null);
        }
        public static SocketListener Register(string ip, string port,ServerConfig config)
        {
            return Register(ip, port,config,null);
        }
        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static SocketListener Register(string ip, string port, ServerConfig config,SocketListener socketserver)
        {
            Tuple<string, string> key = new Tuple<string, string>(ip, port);
            if (!socketservers.TryGetValue(key, out socketserver))
            {
                try
                {
                    if(socketserver == null) socketserver = new SocketListener(key, config);
                    socketservers[key] = socketserver;
                }
                catch (SocketException e)
                {
                    Console.WriteLine("发生异常报错,销毁注册");
                    Console.WriteLine(e.Message + "\n" + e.StackTrace);
                    socketserver.Dispose();
                }
            }
            return socketserver;
        }
        public static SocketListener Get(string ip,string port)
        {
            return Get(new Tuple<string, string>(ip,port));
        }
        public static SocketListener Get(Tuple<string, string> key)
        {
            SocketListener socketserver;
            socketservers.TryGetValue(key, out socketserver);
            return socketserver;
        }
    }
}
