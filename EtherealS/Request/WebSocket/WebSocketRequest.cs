namespace EtherealS.Request.WebSocket
{
    public abstract class WebSocketRequest : Abstract.Request
    {
        #region --属性--
        public new WebSocketRequestConfig Config { get => (WebSocketRequestConfig)config; set => config = value; }

        #endregion

        public WebSocketRequest()
        {
            config = new WebSocketRequestConfig();
        }
    }
}
