using EtherealS.NativeServer;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace EtherealS.Model
{
    /// <summary>
    /// Ethereal中的UserToken基类
    /// </summary>
    public abstract class BaseUserToken
    {
        #region --委托--
        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void ConnectDelegate(BaseUserToken token);
        /// <summary>
        /// 断开连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void DisConnectDelegate(BaseUserToken token);
        #endregion

        #region --事件--
        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectDelegate ConnectEvent;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event ConnectDelegate DisConnectEvent;
        #endregion

        #region --字段--
        /// <summary>
        /// 服务器字段信息
        /// </summary>
        private Tuple<string, string> serverKey;
        /// <summary>
        /// 传输字段信息
        /// </summary>
        private object net;
        #endregion

        #region --属性--
        public Tuple<string, string> ServerKey { get => serverKey; set => serverKey = value; }
        [JsonIgnore]
        public object Net { get => net; set => net = value; }
        #endregion

        #region --方法--
        /// <summary>
        /// 注册Token信息至Tokens表
        /// </summary>
        /// <param name="replace">当已存在Token信息，是否替换</param>
        /// <returns></returns>
        public bool Register(bool replace = false)
        {
            if(!NetCore.Get(serverKey, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{serverKey.Item1}-{serverKey.Item2}Net未找到");
            }
            if (replace)
            {
                net.Tokens.TryRemove(Key, out BaseUserToken token);
                return net.Tokens.TryAdd(Key, this);
            }
            else return net.Tokens.TryAdd(Key, this);
        }
        /// <summary>
        /// 从Tokens表中注销Token信息
        /// </summary>
        /// <returns></returns>
        public bool UnRegister()
        {
            if (!NetCore.Get(serverKey, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{serverKey.Item1}-{serverKey.Item2}Net未找到");
            }
            return net.Tokens.TryRemove(Key, out BaseUserToken value);
        }
        /// <summary>
        /// 得到该Token所属的Tokens表单
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<object, BaseUserToken> GetTokens()
        {
            if (!NetCore.Get(serverKey, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{serverKey.Item1}-{serverKey.Item2}Net未找到");
            }
            return net.Tokens;
        }
        /// <summary>
        /// 得到特定的Token信息
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="key">Token唯一凭据Key</param>
        /// <param name="value">返回的值</param>
        /// <returns></returns>
        public bool GetToken<T>(object key,out T value) where T:BaseUserToken
        {
            if (!NetCore.Get(serverKey, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{serverKey.Item1}-{serverKey.Item2}Net未找到");
            }
            if (net.Tokens.TryGetValue(key, out BaseUserToken result))
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
        /// 主动断开连接
        /// </summary>
        /// <returns></returns>
        public bool DisConnect()
        {
            if(ServerCore.Get(serverKey, out ServerListener server))
            {
                return server.CloseClientSocket((SocketAsyncEventArgs)net);
            }
            return true;
        }
        /// <summary>
        /// 得到某网络层{ip}-{port}中的Tokens表单
        /// </summary>
        /// <param name="serverkey"></param>
        /// <returns></returns>
        public static ConcurrentDictionary<object, BaseUserToken> GetTokens(Tuple<string, string> serverkey)
        {
            if (!NetCore.Get(serverkey, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.RuntimeError, $"{serverkey.Item1}-{serverkey.Item2}Net未找到");
            }
            return net.Tokens;
        }

        #endregion

        #region --抽象属性--
        /// <summary>
        /// Token唯一凭据Key
        /// </summary>
        public abstract object Key { get; set; }
        #endregion

        #region --虚方法--
        /// <summary>
        /// 连接时激活连接事件
        /// </summary>
        public virtual void OnConnect()
        {
            if(ConnectEvent != null)ConnectEvent(this);
        }
        /// <summary>
        /// 断开连接时激活断开连接事件
        /// </summary>
        public virtual void OnDisConnect()
        {
            if(DisConnectEvent != null)DisConnectEvent(this);
        }
        #endregion
    }
}
