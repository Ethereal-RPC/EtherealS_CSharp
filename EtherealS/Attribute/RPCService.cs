using System;
using EtherealS.Extension.Authority;

namespace EtherealS.Attribute
{
    /// <summary>
    /// 作为服务器服务方法的标注类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RPCService : System.Attribute, IAuthoritable
    {
        /// <summary>
        /// 提供自定义MethodId的抽象参数名
        /// </summary>
        private string[] paramters = null;
        /// <summary>
        /// 服务实现IAuthoritable 接口
        /// </summary>
        public object authority = null;
        public string[] Paramters { get => paramters; set => paramters = value; }
        public object Authority { get => authority; set => authority = value; }
    }
}
