using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using EtherealS.Model;
using EtherealS.RPCNet;

namespace EtherealS.NativeServer
{
    public class ServerCore
    {
        private static Dictionary<Tuple<string, string>, ServerListener> socketservers { get; } = new Dictionary<Tuple<string, string>, ServerListener>();

        public static ServerListener Register(string ip, string port,ServerConfig.CreateInstance createMethod)
        {
            return Register(ip, port, new ServerConfig(createMethod),null);
        }
        public static ServerListener Register(string ip, string port,ServerConfig config)
        {
            return Register(ip, port,config,null);
        }
        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static ServerListener Register(string ip, string port, ServerConfig config,ServerListener socketserver)
        {
            Tuple<string, string> key = new Tuple<string, string>(ip, port);
            if (!socketservers.TryGetValue(key, out socketserver))
            { 
                try
                {
                    if(socketserver == null) socketserver = new ServerListener(key, config);
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
        public static bool Get(string ip,string port, out ServerListener socketserver)
        {
            return Get(new Tuple<string, string>(ip,port),out socketserver);
        }
        public static bool Get(Tuple<string, string> key, out ServerListener socketserver)
        {
            return socketservers.TryGetValue(key,out socketserver);
        }

        public static bool UnRegister(Tuple<string,string> key)
        {
            return socketservers.Remove(key);
        }
    }
}
