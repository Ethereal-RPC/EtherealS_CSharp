using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Net.Extension.NetNode.NetNodeClient.Request
{
    public class ServerNodeRequest : EtherealC.Request.WebSocket.WebSocketRequest,IServerNodeRequest
    {
        public ServerNodeRequest()
        {
            name = "ServerNetNodeService";
            types.Add<int>("Int");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
            types.Add<Model.NetNode>("NetNode");
        }
        public virtual bool Register(Model.NetNode node)
        {
            throw new NotImplementedException();
        }
    }
}
