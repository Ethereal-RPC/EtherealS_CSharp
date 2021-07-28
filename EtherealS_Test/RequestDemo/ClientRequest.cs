using EtherealS.Attribute;
using EtherealS_Test.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS_Test.RequestDemo
{
    public interface ClientRequest
    {
        [RPCRequest]
         void Say(User user,User sender, string message);
    }
}
