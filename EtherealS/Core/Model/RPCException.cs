using System;
using EtherealS.Server.Abstract;

namespace EtherealS.Core.Model
{
    /// <summary>
    /// Ethereal-RPC异常类
    /// </summary>
    public class RPCException : Exception
    {
        #region --字段--
        public enum ErrorCode { Core, Runtime, NotEthereal }
        /// <summary>
        /// 错误代码
        /// </summary>
        private ErrorCode errorCode;
        private Net.Abstract.Net net;
        private Server.Abstract.Server server;
        private Service.Abstract.Service service;
        private Request.Abstract.Request request;
        private Token token;
        private Exception exception;
        #endregion

        #region --属性--
        public ErrorCode Error { get => errorCode; set => errorCode = value; }
        public Net.Abstract.Net Net { get => net; set => net = value; }
        public Service.Abstract.Service Service { get => service; set => service = value; }
        public Request.Abstract.Request Request { get => request; set => request = value; }
        public Token Token { get => token; set => token = value; }
        public Exception Exception { get => exception; set => exception = value; }
        public Server.Abstract.Server Server { get => server; set => server = value; }
        #endregion

        public RPCException(string message) : base(message)
        {
            exception = this;
        }
        public RPCException(Exception e) : base("外部库错误\n" + e.Message)
        {
            exception = e;
            errorCode = ErrorCode.NotEthereal;
        }

        public RPCException(ErrorCode errorCode, string message) : base(message)
        {
            exception = this;
            this.errorCode = errorCode;
        }
    }
}
