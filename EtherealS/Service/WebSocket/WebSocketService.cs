using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using EtherealS.Core.Model;
using EtherealS.Server.Abstract;
using EtherealS.Service.Abstract;

namespace EtherealS.Service.WebSocket
{
    public class WebSocketService:Abstract.Service
    {
        #region --属性--
        public new WebSocketServiceConfig Config { get => (WebSocketServiceConfig)config; set => config = value; }


        #endregion

        #region --方法--

        public WebSocketService(string name, AbstractTypes types) : base(name,types)
        {
            config = new WebSocketServiceConfig();
        }

        #endregion
    }
}
