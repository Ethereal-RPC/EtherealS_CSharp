using EtherealS.NativeServer;
using EtherealS.RPCNet;
using Newtonsoft.Json;
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
        private string netName;
        /// <summary>
        /// 传输字段信息
        /// </summary>
        private object net;
        #endregion

        #region --属性--

        [JsonIgnore]
        public object Net { get => net; set => net = value; }
        public string NetName { get => netName; set => netName = value; }

        /// <summary>
        /// Token唯一凭据Key
        /// </summary>
        public abstract object Key { get; set; }
        #endregion

        #region --方法--
        /// <summary>
        /// 注册Token信息至Tokens表
        /// </summary>
        /// <param name="replace">当已存在Token信息，是否替换</param>
        /// <returns></returns>
        public bool Register(bool replace = false)
        {
            if(!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Runtime, $"{netName}Net未找到");
            }
            if (replace)
            {
                net.Tokens.TryRemove(Key, out BaseUserToken token);
            }
            return net.Tokens.TryAdd(Key, this);
        }
        /// <summary>
        /// 从Tokens表中注销Token信息
        /// </summary>
        /// <returns></returns>
        public bool UnRegister()
        {
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Runtime, $"{netName}Net未找到");
            }
            return net.Tokens.TryRemove(Key, out BaseUserToken value);
        }
        /// <summary>
        /// 得到该Token所属的Tokens表单
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<object, BaseUserToken> GetTokens()
        {
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Runtime, $"{netName}Net未找到");
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
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Runtime, $"{netName}Net未找到");
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
            if(ServerCore.Get(netName, out ServerListener server))
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
        public static ConcurrentDictionary<object, BaseUserToken> GetTokens(string netName)
        {
            if (!NetCore.Get(netName, out Net net))
            {
                throw new RPCException(RPCException.ErrorCode.Runtime, $"{netName}Net未找到");
            }
            return net.Tokens;
        }
        /// <summary>
        /// 偷梁换柱，可以使用任意继承BaseUserToken的类型替换当前Token
        /// tip:新Token不会继承原有Token的Key，且原有Token将执行注销(UnRegister).
        /// </summary>
        /// <param name="serverkey"></param>
        /// <returns></returns>
        public bool ReplaceToken(BaseUserToken token)
        {
            DataToken dataToken = (net as DataToken);
            token.net = net;
            token.netName = netName;
            //如果在池中注册过，将池数据也替换掉
            if(GetToken(Key,out BaseUserToken temp))
            {
                UnRegister();
            }
            dataToken.Token = token;
            return true;
        }
        #endregion


        #region --虚方法--
        /// <summary>
        /// 连接时激活连接事件
        /// </summary>
        public virtual void OnConnect()
        {
            ConnectEvent?.Invoke(this);
        }
        /// <summary>
        /// 断开连接时激活断开连接事件
        /// </summary>
        public virtual void OnDisConnect()
        {
            DisConnectEvent?.Invoke(this);
        }
        #endregion
    }
}
