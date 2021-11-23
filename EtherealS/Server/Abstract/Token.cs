using EtherealS.Core;
using EtherealS.Core.BaseCore;
using EtherealS.Core.Model;
using EtherealS.Server.Interface;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace EtherealS.Server.Abstract
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Token : BaseCore,IToken
    {

        #region --委托--
        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void ConnectDelegate(Token token);
        /// <summary>
        ///  断开连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void DisConnectDelegate(Token token);

        #endregion

        #region --事件字段--

        #endregion

        #region --事件属性--
        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectDelegate ConnectEvent;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event DisConnectDelegate DisConnectEvent;
        #endregion

        #region --字段--

        protected bool canRequest = false;
        protected object key;
        protected Service.Abstract.Service service;

        #endregion

        #region --属性--
        public bool CanRequest { get => canRequest; set => canRequest = value; }
        public Service.Abstract.Service Service { get => service; set => service = value; }
        public object Key { get => key; set => key = value; }
        #endregion

        #region --方法--
        /// <summary>
        /// 注册Token信息至Tokens表
        /// </summary>
        /// <param name="replace">当已存在Token信息，是否替换</param>
        /// <returns></returns>
        public bool Register(bool replace = false)
        {
            if (replace)
            {
                service.Tokens.TryRemove(Key, out Token token);
            }
            return service.Tokens.TryAdd(Key, this);
        }
        /// <summary>
        /// 从Tokens表中注销Token信息
        /// </summary>
        /// <returns></returns>
        public bool UnRegister()
        {
            if (Key == null) return true;
            return service.Tokens.TryRemove(Key, out Token value);
        }
        /// <summary>
        /// 得到该Token所属的Tokens表单
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<object, Token> GetTokens()
        {
            return service.Tokens;
        }
        /// <summary>
        /// 得到特定的Token信息
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="key">Token唯一凭据Key</param>
        /// <param name="value">返回的值</param>
        /// <returns></returns>
        public bool GetToken<T>(object key, out T value) where T : Token
        {
            if (service.Tokens.TryGetValue(key, out Token result))
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
        /// <summary>
        /// 得到某网络层{ip}-{port}中的Tokens表单
        /// </summary>
        /// <param name="serverkey"></param>
        /// <returns></returns>
        public ConcurrentDictionary<object, Token> GetTokens(string netName)
        {
            return service.Tokens;
        }
        #endregion

        #region --网络方法--

        public abstract void DisConnect(string reason);

        internal abstract void SendClientResponse(ClientResponseModel response);
        internal abstract void SendServerRequest(ServerRequestModel request);

        /// <summary>
        /// 连接时激活连接事件
        /// </summary>
        public void OnConnect()
        {
            ConnectEvent?.Invoke(this);
        }
        /// <summary>
        /// 断开连接时激活断开连接事件
        /// </summary>
        public void OnDisConnect()
        {
            DisConnectEvent?.Invoke(this);
        }
        #endregion
    }
}
