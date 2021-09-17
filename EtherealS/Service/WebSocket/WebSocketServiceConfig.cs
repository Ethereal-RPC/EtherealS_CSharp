using EtherealS.Core.Model;
using EtherealS.Service.Abstract;

namespace EtherealS.Service.WebSocket
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
