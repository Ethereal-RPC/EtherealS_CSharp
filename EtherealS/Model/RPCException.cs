using System;

namespace EtherealS.Model
{
    /// <summary>
    /// Ethereal-RPC异常类
    /// </summary>
    class RPCException : Exception
    {
        public enum ErrorCode { RegisterError, RuntimeError }
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
