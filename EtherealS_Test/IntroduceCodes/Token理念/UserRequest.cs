using EtherealS.Annotation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS_Test.IntroduceCodes
{
    public interface UserRequest
    {
        [RPCRequest]
        public bool Login(string username, string password);

        [RPCRequest]
        public DateTime GetLoginTime(User user);
    }
}
