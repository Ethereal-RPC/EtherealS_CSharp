using EtherealS_Test.Model;
using System;
using System.Collections.Generic;
using System.Text;
using EtherealS.Request.Attribute;

namespace EtherealS_Test.RequestDemo
{
    public interface ClientRequest
    {
        [Request]
         void Say(User user,User sender, string message);
    }
}
