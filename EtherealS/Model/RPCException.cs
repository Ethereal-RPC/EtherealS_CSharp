using EtherealS.NativeServer;
using EtherealS.NativeServer.Abstract;
using EtherealS.RPCNet;
using EtherealS.RPCRequest;
using EtherealS.RPCService;
using System;

namespace EtherealS.Model
{
    /// <summary>
    /// Ethereal-RPC异常类
    /// </summary>
    public class RPCException : Exception
    {
        #region --字段--
        public enum ErrorCode { Core, Runtime }
        /// <summary>
        /// 错误代码
        /// </summary>
        private ErrorCode errorCode;
        private Net net;
        private Server server;
        private Service service;
        private Request request;
        private BaseToken token;
        private Exception exception;
        #endregion

        #region --属性--
        public ErrorCode Error { get => errorCode; set => errorCode = value; }
        public Net Net { get => net; set => net = value; }
        public Service Service { get => service; set => service = value; }
        public Request Request { get => request; set => request = value; }
        public BaseToken Token { get => token; set => token = value; }
        public Exception Exception { get => exception; set => exception = value; }
        public Server Server { get => server; set => server = value; }
        #endregion

        public RPCException(string message) : base(message)
        {

        }
        public RPCException(Exception e,string message) : base(message)
        {
            this.exception = e;
        }

        public RPCException(ErrorCode errorCode, string message) : base(message)
        {
            this.errorCode = errorCode;
        }
    }
}
