using System;

namespace EtherealS.Request.Attribute
{
    /// <summary>
    /// 作为服务器请求方法的标注类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Request : System.Attribute
    {
        [Flags]
        public enum InvokeTypeFlags
        {
            Local = 0x1,
            Remote = 0x2,
            All = 0x4,
        }

        /// <summary>
        /// 提供自定义MethodId的抽象参数名
        /// </summary>
        private string[] paramters = null;

        private InvokeTypeFlags invokeType = InvokeTypeFlags.Remote;

        public InvokeTypeFlags InvokeType { get => invokeType; set => invokeType = value; }
        public string[] Paramters { get => paramters; set => paramters = value; }
    }
}
