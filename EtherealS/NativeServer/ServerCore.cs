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
        public static bool Get(string netName, out ServerListener socketserver)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return Get(net, out socketserver);
            }
            else
            {
                socketserver = null;
                return false;
            }
        }
        public static bool Get(Net net, out ServerListener socketserver)
        {
            socketserver = net.Server;
            if (net.Server != null) return true;
            else return false;
        }

        public static ServerListener Register(Net net, string ip, string port,ServerConfig.CreateInstance createMethod)
        {
            return Register(net, ip, port, new ServerConfig(createMethod),null);
        }
        public static ServerListener Register(Net net, string ip, string port,ServerConfig config)
        {

            return Register(net, ip, port,config,null);
        }
        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static ServerListener Register(Net net, string ip, string port,ServerConfig config,ServerListener socketserver)
        {
            Tuple<string, string> key = new Tuple<string, string>(ip, port);
            if (net.Server == null)
            {
                if (socketserver == null) socketserver = new ServerListener(net, key, config);
                net.Server = socketserver;
                net.Server.LogEvent += net.OnServerLog;
                net.Server.ExceptionEvent += net.OnServerException;
            }
            return socketserver;
        }

        public static bool UnRegister(string netName)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return UnRegister(net);
            }
            else
            {
                return true;
            }
        }
        public static bool UnRegister(Net net)
        {
            net.Server.LogEvent -= net.OnServerLog;
            net.Server.ExceptionEvent -= net.OnServerException;
            net.Server.Stop();
            net.Server.Dispose();
            net.Server = null;
            net.ServerRequestSend = null;
            net.ClientResponseSend = null;
            return true;
        }
    }
}
