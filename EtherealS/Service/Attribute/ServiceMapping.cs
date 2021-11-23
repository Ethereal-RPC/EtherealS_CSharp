using EtherealS.Service.Extension.Authority;
using System;

namespace EtherealS.Service.Attribute
{
    /// <summary>
    /// 作为服务器服务方法的标注类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceMapping : System.Attribute, IAuthoritable
    {
        public ServiceMapping(string Mapping)
        {
            mapping = Mapping;
        }
        /// <summary>
        /// 服务实现IAuthoritable 接口
        /// </summary>
        private object authority = null;
        private string mapping = null;
        public object Authority { get => authority; set => authority = value; }
        public string Mapping { get => mapping; set => mapping = value; }
    }
}
