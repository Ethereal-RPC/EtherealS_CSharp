using System;
using System.Collections.Concurrent;
using System.Reflection;
using EtherealS.Model;
using EtherealS.RPCService;

namespace EtherealS.RPCNet
{
    public class NetConfig
    {
        private ConcurrentDictionary<object, BaseUserToken> tokens = new ConcurrentDictionary<object, BaseUserToken>();
        #region --委托--
        public delegate bool InterceptorDelegate(Service service,MethodInfo method,BaseUserToken token);
        public delegate void ClientRequestReceiveDelegate(Tuple<string, string> key,BaseUserToken token, ClientRequestModel request);
        public delegate void ClientRequestReceiveVoidDelegate(Tuple<string, string> key, BaseUserToken token, ClientRequestModel request);
        public delegate void ServerRequestSendDelegate(BaseUserToken token, ServerRequestModel request);
        public delegate void ClientResponseSendDelegate(BaseUserToken token, ClientResponseModel response);
        #endregion

        #region --事件--
        public event InterceptorDelegate InterceptorEvent;
        #endregion

        #region --字段--
        private ClientRequestReceiveDelegate clientRequestReceive;
        private ServerRequestSendDelegate serverRequestSend;
        private ClientResponseSendDelegate clientResponseSend;
        #endregion

        #region --属性-
        public ClientRequestReceiveDelegate ClientRequestReceive { get => clientRequestReceive; set => clientRequestReceive = value; }
        public ServerRequestSendDelegate ServerRequestSend { get => serverRequestSend; set => serverRequestSend = value; }
        public ClientResponseSendDelegate ClientResponseSend { get => clientResponseSend; set => clientResponseSend = value; }
        public ConcurrentDictionary<object, BaseUserToken> Tokens { get => tokens; set => tokens = value; }

        #endregion

        #region --方法--
        public bool OnInterceptor(Service service,MethodInfo method,BaseUserToken token)
        {
            if (InterceptorEvent != null)
            {
                foreach (InterceptorDelegate item in InterceptorEvent.GetInvocationList())
                {
                    if (!item.Invoke(service, method, token)) return false;
                }
                return true;
            }
            else return true;
        }


        #endregion
    }
}
