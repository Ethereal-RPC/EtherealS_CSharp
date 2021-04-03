using System;

namespace EtherealS.Model
{
    class RPCException : Exception
    {
        public enum ErrorCode { NotFoundBaseUserTokenInstanceMethod, NotFoundNetConfig,NotFoundMethod,NotFoundService,RegisterError }
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
