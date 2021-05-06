using System;

namespace EtherealS.Attribute
{
    /// <summary>
    /// 作为服务器请求方法的标注类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RPCRequest : System.Attribute
    {
        /// <summary>
        /// 提供自定义MethodId的抽象参数名
        /// </summary>
        private string[] paramters = null;
        
        public string[] Paramters { get => paramters; set => paramters = value; }
    }
}
