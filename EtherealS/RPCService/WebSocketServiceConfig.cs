using EtherealS.Core.Model;
using EtherealS.NativeServer;
using System.Reflection;

namespace EtherealS.RPCService
{
    /// <summary>
    /// 服务配置项
    /// </summary>
    public class WebSocketServiceConfig : ServiceConfig
    {
        public WebSocketServiceConfig(RPCTypeConfig type) : base(type)
        {
        }
    }
}
