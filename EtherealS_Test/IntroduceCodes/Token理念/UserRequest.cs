using EtherealS.Annotation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS_Test.IntroduceCodes
{
    /* 客户端远程请求接口 */
    public interface UserRequest
    {
        [RPCRequest]
        public bool Login(string username, string password);

        [RPCRequest]
        public DateTime GetLoginTime(User user);
    }
}
