using EtherealS.NativeServer;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace EtherealS.Model
{
    public abstract class BaseUserToken
    {
        #region --委托--
        public delegate void ConnectDelegate(BaseUserToken token);
        public delegate void DisConnectDelegate(BaseUserToken token);
        #endregion

        #region --事件--
        public event ConnectDelegate ConnectEvent;
        public event ConnectDelegate DisConnectEvent;
        #endregion

        #region --字段--
        private Tuple<string, string> serverKey;
        private object net;
        #endregion

        #region --属性--
        public Tuple<string, string> ServerKey { get => serverKey; set => serverKey = value; }
        [JsonIgnore]
        public object Net { get => net; set => net = value; }
        #endregion

        #region --方法--

        public bool Register(bool replace = false)
        {
            if (replace)
            {
                NetCore.GetTokens(serverKey).TryRemove(Key, out BaseUserToken token);
                return NetCore.GetTokens(serverKey).TryAdd(Key, this);
            }
            else return NetCore.GetTokens(serverKey).TryAdd(Key, this);
        }
        public bool UnRegister()
        {
            return NetCore.GetTokens(serverKey).TryRemove(Key, out BaseUserToken value);
        }
        public ConcurrentDictionary<object, BaseUserToken> GetTokens()
        {
            return NetCore.GetTokens(serverKey);
        }
        public bool GetToken<T>(object key,out T value) where T:BaseUserToken
        {
            if (NetCore.GetTokens(serverKey).TryGetValue(key, out BaseUserToken result))
            {
                value = (T)result;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public bool DisConnect()
        {
            if(ServerCore.Get(serverKey, out ServerListener server))
            {
                return server.CloseClientSocket((SocketAsyncEventArgs)net);
            }
            return true;
        }

        public static ConcurrentDictionary<object, BaseUserToken> GetTokens(Tuple<string, string> serverkey)
        {
            return NetCore.GetTokens(serverkey);
        }



        #endregion

        #region --抽象属性--
        public abstract object Key { get; set; }
        #endregion

        #region --虚方法--
        public virtual void OnConnect()
        {
            if(ConnectEvent != null)ConnectEvent(this);
        }

        public virtual void OnDisConnect()
        {
            if(DisConnectEvent != null)DisConnectEvent(this);
        }
        #endregion
    }
}
