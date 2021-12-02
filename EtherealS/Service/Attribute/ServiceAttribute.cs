using EtherealS.Service.Extension.Authority;
using System;

namespace EtherealS.Service.Attribute
{
    /// <summary>
    /// 作为服务器服务方法的标注类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : System.Attribute, IAuthoritable
    {
        /// <summary>
        /// 服务实现IAuthoritable 接口
        /// </summary>
        public object authority = null;
        public object Authority { get => authority; set => authority = value; }
    }
}
