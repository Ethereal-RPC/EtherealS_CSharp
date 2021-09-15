using EtherealS.Core.Interface;
using EtherealS.Model;
using EtherealS.NativeServer;
using EtherealS.NativeServer.Abstract;
using EtherealS.RPCRequest;
using EtherealS.RPCService;
using System;

namespace EtherealS.RPCNet
{
    public interface INet : ILogEvent, IExceptionEvent
    {
        #region --方法--
        /// <summary>
        /// 部署节点
        /// </summary>
        /// <returns></returns>
        public bool Publish();

        public ClientResponseModel ClientRequestReceiveProcess(BaseToken token, ClientRequestModel request);

        #endregion
    }
}
