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
            else throw new RPCException(RPCException.ErrorCode.Core, $"{netName}Net未找到");
        }
        public static bool Get(Net net, out ServerListener socketserver)
        {
            socketserver = net.Server;
            if (net.Server != null) return true;
            else return false;
        }

        public static ServerListener Register(string ip, string port,Net net,ServerConfig.CreateInstance createMethod)
        {
            return Register(ip, port,net, new ServerConfig(createMethod),null);
        }
        public static ServerListener Register(string ip, string port, Net net,ServerConfig config)
        {
            return Register(ip, port,net,config,null);
        }
        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static ServerListener Register(string ip, string port, Net net,ServerConfig config,ServerListener socketserver)
        {
            Tuple<string, string> key = new Tuple<string, string>(ip, port);
            if (net.Server == null)
            {
                if (socketserver == null) socketserver = new ServerListener(net, key, config);
                net.Server = socketserver;
            }
            return socketserver;
        }

        public static bool UnRegister(string netName)
        {
            if (NetCore.Get(netName, out Net net))
            {
                return UnRegister(net);
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"{netName}Net未找到");
        }
        public static bool UnRegister(Net net)
        {
            net.Server.Dispose();
            net.Server = null;
            net.ServerRequestSend = null;
            net.ClientResponseSend = null;
            return true;
        }
    }
}
