using EtherealS.Core.Interface;
using EtherealS.Core.Model;
using EtherealS.Server.Abstract;

namespace EtherealS.Net.Interface
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
