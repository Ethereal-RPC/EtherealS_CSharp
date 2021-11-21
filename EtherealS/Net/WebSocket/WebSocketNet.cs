using System.Threading;

namespace EtherealS.Net.WebSocket
{
    public class WebSocketNet : Abstract.Net
    {
        #region --属性--
        public new WebSocketNetConfig Config { get => (WebSocketNetConfig)config; set => config = value; }
        public AutoResetEvent sign = new AutoResetEvent(false);
        #endregion

        #region --方法--

        #endregion

        public WebSocketNet(string name) : base(name)
        {
            this.config = new WebSocketNetConfig();
            this.Type = NetType.WebSocket;
        }
    }
}
