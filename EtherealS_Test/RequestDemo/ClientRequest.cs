using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherealS.Request.WebSocket;
using EtherealS_Test.Model;

namespace EtherealS_Test.RequestDemo
{
    public class ClientRequest:WebSocketRequest,IClientRequest
    {
        public override void Initialize()
        {

        }

        public virtual string Say(User user, User sender, string message)
        {
            return "sd";
        }

        public override void UnInitialize()
        {

        }
    }
}
