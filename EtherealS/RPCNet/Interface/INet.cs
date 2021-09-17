using EtherealS.Core.Interface;
using EtherealS.Core.Model;
using EtherealS.NativeServer;
using EtherealS.NativeServer.Abstract;

namespace EtherealS.RPCNet.Interface
{
    public interface INet : ILogEvent, IExceptionEvent
    {
        #region --方法--
        /// <summary>
        /// 部署节点
        /// </summary>
        /// <returns></returns>
        public bool Publish();

        public ClientResponseModel ClientRequestReceiveProcess(Token token, ClientRequestModel request);

        #endregion
    }
}
