using EtherealS.Core.Model;
using EtherealS.RPCService.Abstract;

namespace EtherealS.RPCService.WebSocket
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
