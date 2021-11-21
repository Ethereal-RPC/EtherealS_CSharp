using EtherealS.Server.WebSocket;

namespace EtherealS.Service.WebSocket
{
    public abstract class WebSocketService : Abstract.Service
    {
        #region --属性--
        public new WebSocketServiceConfig Config { get => (WebSocketServiceConfig)config; set => config = value; }


        #endregion

        #region --方法--

        public WebSocketService()
        {
            config = new WebSocketServiceConfig();
            tokenCreateInstance = () => new WebSocketToken();
        }

        #endregion
    }
}
