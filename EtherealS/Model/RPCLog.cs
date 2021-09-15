using EtherealS.NativeServer;
using EtherealS.NativeServer.Abstract;
using EtherealS.RPCNet;
using EtherealS.RPCRequest;
using EtherealS.RPCService;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS.Model
{
    public class RPCLog
    {
        public enum LogCode { Core, Runtime }

        #region --字段--
        private string message;
        private LogCode code;

        private Net net;
        private Server server;
        private Service service;
        private Request request;
        private BaseToken token;
        #endregion



        #region --属性--
        public string Message { get => message; set => message = value; }
        public LogCode Code { get => code; set => code = value; }
        public Net Net { get => net; set => net = value; }
        public Service Service { get => service; set => service = value; }
        public Request Request { get => request; set => request = value; }
        public BaseToken Token { get => token; set => token = value; }
        public Server Server { get => server; set => server = value; }
        #endregion

        public RPCLog(LogCode code,string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}
