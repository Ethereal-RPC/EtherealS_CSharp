using EtherealC.Core.Model;
using EtherealS.Net.NetNode.NetNodeClient.Request;

namespace EtherealS.Net.NetNode.NetNodeClient.Service
{
    public class ClientNodeService:EtherealC.Service.WebSocket.WebSocketService
    {
        #region --字段--
        private IServerNodeRequest serverNodeRequest;
        #endregion

        #region --属性--
        public IServerNodeRequest ServerNodeRequest { get => serverNodeRequest; set => serverNodeRequest = value; }
        #endregion

        #region --RPC方法--


        #endregion

        #region --普通方法--
        
        #endregion


    }
}
