using System;
using System.Reflection;
using EtherealS.Service.Extension.Authority;

namespace EtherealS.Request.Attribute
{
    /// <summary>
    /// 作为服务器服务方法的标注类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Request : System.Attribute
    {

    }
}
