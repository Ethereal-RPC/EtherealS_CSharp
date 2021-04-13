using System;

namespace EtherealS.Model
{
    class RPCException : Exception
    {
        public enum ErrorCode { RegisterError, RuntimeError }
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
