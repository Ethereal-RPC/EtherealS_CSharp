using System.Collections.Concurrent;
using EtherealS.Core.Delegates;
using EtherealS.Core.Model;
using EtherealS.Net;
using EtherealS.Server.Interface;

namespace EtherealS.Server.Abstract
{
    public abstract class BaseToken: IBaseToken
    {

        #region --委托--
        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void ConnectDelegate(BaseToken token);
        /// <summary>
        ///     
        /// </summary>
        /// <param name="token"></param>
        public delegate void DisConnectDelegate(BaseToken token);

        #endregion

        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
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
        /// <summary>
        /// 日志输出事件
        /// </summary>
        internal event OnLogDelegate LogEvent
        {
            add
            {
                logEvent -= value;
                logEvent += value;
            }
            remove
            {
                logEvent -= value;
            }
        }
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        internal event OnExceptionDelegate ExceptionEvent
        {
            add
            {
                exceptionEvent -= value;
                exceptionEvent += value;
            }
            remove
            {
                exceptionEvent -= value;
            }
        }
        #endregion

        #region --字段--
        protected string netName;
        protected ServerConfig config;
        protected bool isWebSocket;
        #endregion

        #region --属性--
        public abstract object Key { get; set; }
        public bool IsWebSocket { get => isWebSocket; set => isWebSocket = value; }
        public string NetName { get => netName; set => netName = value; }
        public ServerConfig Config { get => config; set => config = value; }
        #endregion

        #region --方法--
        /// <summary>
        /// 注册Token信息至Tokens表
        /// </summary>
        /// <param name="replace">当已存在Token信息，是否替换</param>
        /// <returns></returns>
        public bool Register(bool replace = false)
        {
            if (!NetCore.Get(NetName, out Net.Abstract.Net net))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{NetName}Net未找到");
            }
            if (replace)
            {
                net.Tokens.TryRemove(Key, out BaseToken token);
            }
            return net.Tokens.TryAdd(Key, this);
        }
        /// <summary>
        /// 从Tokens表中注销Token信息
        /// </summary>
        /// <returns></returns>
        public bool UnRegister()
        {
            if (Key == null) return true;
            if (!NetCore.Get(NetName, out Net.Abstract.Net net))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{NetName}Net未找到");
            }
            return net.Tokens.TryRemove(Key, out BaseToken value);
        }
        /// <summary>
        /// 得到该Token所属的Tokens表单
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<object, BaseToken> GetTokens()
        {
            if (!NetCore.Get(NetName, out Net.Abstract.Net net))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{NetName}Net未找到");
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
        public bool GetToken<T>(object key, out T value) where T : BaseToken
        {
            if (!NetCore.Get(NetName, out Net.Abstract.Net net))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{NetName}Net未找到");
            }
            if (net.Tokens.TryGetValue(key, out BaseToken result))
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
        public ConcurrentDictionary<object, BaseToken> GetTokens(string netName)
        {
            if (!NetCore.Get(netName, out Net.Abstract.Net net))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"{netName}Net未找到");
            }
            return net.Tokens;
        }
        #endregion

        #region --网络方法--

        public abstract void DisConnect(string reason);

        internal abstract void SendClientResponse(ClientResponseModel response);
        internal abstract void SendServerRequest(ServerRequestModel request);

        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            if (exceptionEvent != null)
            {
                e.Token = this;
                exceptionEvent?.Invoke(e);
            }
        }

        public void OnLog(TrackLog.LogCode code, string message)
        {
            OnLog(new TrackLog(code, message));
        }
        public void OnLog(TrackLog log)
        {
            if (logEvent != null)
            {
                log.Token = this;
                logEvent?.Invoke(log);
            }
        }
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
