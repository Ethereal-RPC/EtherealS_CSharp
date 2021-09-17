using EtherealS.Core.Model;
using EtherealS.Net;
using EtherealS.Server.Abstract;
using EtherealS.Server.WebSocket;

namespace EtherealS.Server
{
    public class ServerCore
    {
        public static bool Get(string netName, out Server.Abstract.Server socketserver)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return Get(net, out socketserver);
            }
            else
            {
                socketserver = null;
                return false;
            }
        }
        public static bool Get(Net.Abstract.Net net, out Server.Abstract.Server socketserver)
        {
            socketserver = net.Server;
            if (net.Server != null) return true;
            else return false;
        }

        public static Server.Abstract.Server Register(Net.Abstract.Net net, string[] prefixes, ServerConfig.CreateInstance createMethod)
        {
            if (net.Type == Net.Abstract.Net.NetType.WebSocket)
            {
                return Register(net, prefixes, new WebSocketServerConfig(createMethod), null);
            }
            else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{net.Type}的Server-Register处理");

        }
        public static Server.Abstract.Server Register(Net.Abstract.Net net, string[] prefixes, ServerConfig config)
        {

            return Register(net,prefixes,config,null);
        }
        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static Server.Abstract.Server Register(Net.Abstract.Net net, string[] prefixes, ServerConfig config, Server.Abstract.Server server)
        {
            if (net.Server == null && net is Net.Abstract.Net)
            {
                if (net.Type == Net.Abstract.Net.NetType.WebSocket)
                {
                    server = new WebSocketServer(net.Name, prefixes, config);
                }
                else throw new RPCException(RPCException.ErrorCode.Core, $"未有针对{net.Type}的Server-Register处理");
                net.Server = server;
                server.LogEvent += net.OnLog;
                server.ExceptionEvent += net.OnException;
            }
            return server;
        }

        public static bool UnRegister(string netName)
        {
            if (NetCore.Get(netName, out Net.Abstract.Net net))
            {
                return UnRegister(net);
            }
            else
            {
                return true;
            }
        }
        public static bool UnRegister(Net.Abstract.Net net)
        {
            if(net != null)
            {
                net.Server.LogEvent -= net.OnLog;
                net.Server.ExceptionEvent -= net.OnException;
                net.Server.Close();
                net.Server = null;
            }
            return true;
        }
    }
}
