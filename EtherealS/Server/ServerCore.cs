using EtherealS.Net;

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

        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="serverIp">远程服务IP</param>
        /// <param name="port">远程服务端口</param>
        /// <returns>客户端</returns>
        public static Abstract.Server Register(Net.Abstract.Net net, Abstract.Server server, bool startServer = true)
        {
            if (net.Server == null)
            {
                net.Server = server;
                server.Net = net;
                server.LogEvent += net.OnLog;
                server.ExceptionEvent += net.OnException;
                if (startServer)
                {
                    server.Start();
                }
            }
            return server;
        }
        public static bool UnRegister(Abstract.Server server)
        {
            server.LogEvent -= server.Net.OnLog;
            server.ExceptionEvent -= server.Net.OnException;
            server.Net.Server = null;
            server.Net = null;
            server.Close();
            return true;
        }
    }
}
