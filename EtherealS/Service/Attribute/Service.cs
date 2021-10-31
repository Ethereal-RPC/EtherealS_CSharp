using System;
using EtherealS.Service.Extension.Authority;

namespace EtherealS.Service.Attribute
{
    /// <summary>
    /// 作为服务器服务方法的标注类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Service : System.Attribute, IAuthoritable
    {
        /// <summary>
        /// 服务实现IAuthoritable 接口
        /// </summary>
        public object authority = null;
        private bool plugin = true;
        public object Authority { get => authority; set => authority = value; }
        public bool Plugin { get => plugin; set => plugin = value; }
    }
}
