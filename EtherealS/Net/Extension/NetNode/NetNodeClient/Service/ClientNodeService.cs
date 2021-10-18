using EtherealC.Core.Model;
using EtherealS.Net.Extension.NetNode.NetNodeClient.Request;

namespace EtherealS.Net.Extension.NetNode.NetNodeClient.Service
{
    public class ClientNodeService:EtherealC.Service.WebSocket.WebSocketService
    {
        #region --字段--

        public ClientNodeService()
        {
            name = "ClientNetNodeService";
            types.Add<int>("Int");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
            types.Add<Model.NetNode>("NetNode");
        }
        #endregion

        #region --属性--

        #endregion

        #region --RPC方法--


        #endregion

        #region --普通方法--
        
        #endregion


    }
}
