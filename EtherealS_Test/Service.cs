using EtherealS.Annotation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS_Test
{
    public class Service
    {
        [RPCService]
        public string Hello(Token token,string str)
        {
            return str + "你好！";
        }
    }
}
