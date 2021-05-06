using EtherealS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS_Test.IntroduceCodes.RPCType抽象类
{
    class Test
    {
        public void test()
        {
            RPCTypeConfig types = new RPCTypeConfig();//RPCType类型的集合
            RPCType type = new RPCType(typeof(string),"String");//生成一个C# String类型与RPC “String”中间抽象类型的绑定类
            types.Add(type);//注册该RPCType类型
        }
    }
}
