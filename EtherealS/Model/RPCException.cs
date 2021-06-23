using System;

namespace EtherealS.Model
{
    /// <summary>
    /// Ethereal-RPC异常类
    /// </summary>
    public class RPCException : Exception
    {
        public enum ErrorCode { Core, Runtime }
        /// <summary>
        /// 错误代码
        /// </summary>
        private ErrorCode errorCode;

        public ErrorCode Error { get => errorCode; set => errorCode = value; }

        public RPCException(string message) : base(message)
        {

        }

        public RPCException(ErrorCode errorCode, string message) : base(message)
        {
            this.errorCode = errorCode;
        }
    }
}
