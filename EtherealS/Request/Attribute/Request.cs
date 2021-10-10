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

        private InvokeTypeFlags invokeType = InvokeTypeFlags.Remote;

        public InvokeTypeFlags InvokeType { get => invokeType; set => invokeType = value; }
    }
}
