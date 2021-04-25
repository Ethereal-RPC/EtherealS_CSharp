using System;
using EtherealS.Extension.Authority;

namespace EtherealS.Annotation
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RPCService : Attribute,IAuthoritable
    {
        private string[] paramters = null;
        public object authority = null;
        private bool token = true;
        public string[] Paramters { get => paramters; set => paramters = value; }
        public object Authority { get => authority; set => authority = value; }
        public bool Token { get => token; set => token = value; }
    }
}
