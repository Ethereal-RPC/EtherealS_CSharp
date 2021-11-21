﻿using System;

namespace EtherealS.Request.Attribute
{
    /// <summary>
    /// 作为服务器请求方法的标注类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestMapping : System.Attribute
    {
        public RequestMapping(string Mapping)
        {
            this.mapping = Mapping;
        }
        [Flags]
        public enum InvokeTypeFlags
        {
            Local = 0x1,
            Remote = 0x2
        }
        private string mapping = null;
        private InvokeTypeFlags invokeType = InvokeTypeFlags.Remote;

        public InvokeTypeFlags InvokeType { get => invokeType; set => invokeType = value; }
        public string Mapping { get => mapping; set => mapping = value; }
    }
}
