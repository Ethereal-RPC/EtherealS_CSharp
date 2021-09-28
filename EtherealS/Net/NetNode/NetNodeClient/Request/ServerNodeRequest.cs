using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Net.NetNode.NetNodeClient.Request
{
    public class ServerNodeRequest : EtherealC.Request.WebSocket.WebSocketRequest,IServerNodeRequest
    {
        public virtual bool Register(Model.NetNode node)
        {
            throw new NotImplementedException();
        }
    }
}
