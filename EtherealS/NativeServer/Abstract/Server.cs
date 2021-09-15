using EtherealS.Model;
using EtherealS.NativeServer.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using static EtherealS.Core.Delegate.Delegates;

namespace EtherealS.NativeServer.Abstract
{
    public abstract class Server:IServer
    {

        #region --委托--

        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void ListenerSuccessDelegate(Server listener);
        /// <summary>
        ///     
        /// </summary>
        /// <param name="token"></param>
        public delegate void ListenerFailDelegate(Server listener);

        #endregion

        #region --事件字段--
        private OnLogDelegate logEvent;
        private OnExceptionDelegate exceptionEvent;
        #endregion

        #region --事件属性--
        /// <summary>
        /// 日志输出事件
        /// </summary>
        public event OnLogDelegate LogEvent
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
        public event OnExceptionDelegate ExceptionEvent
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

        /// <summary>
        /// 连接事件
        /// </summary>
        public event ListenerSuccessDelegate ListenerSuccessEvent;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event ListenerFailDelegate ListenerFailEvent;
        #endregion

        #region --字段--
        protected string netName;
        protected ServerConfig config;
        protected HttpListener listener;
        protected CancellationToken cancellationToken = CancellationToken.None;
        protected List<string> prefixes;
        #endregion

        #region --属性--

        public HttpListener Listener { get => listener; set => listener = value; }
        public List<string> Prefixes { get => prefixes; set => prefixes = value; }
        #endregion

        public abstract void Start();
        public abstract void Close();

        internal abstract void SendErrorToClient(HttpListenerContext context, Error.ErrorCode code, string message);

        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(Exception e)
        {
            if (exceptionEvent != null)
            {
                if (e is not RPCException)
                {
                    e = new RPCException(e, e.Message);
                }
                (e as RPCException).Server = this;
                exceptionEvent.Invoke(e);
            }
        }

        public void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }
        public void OnLog(RPCLog log)
        {
            if (logEvent != null)
            {
                log.Server = this;
                logEvent.Invoke(log);
            }
        }
        /// <summary>
        /// 连接时激活连接事件
        /// </summary>
        protected void OnListenerSuccess()
        {
            ListenerSuccessEvent?.Invoke(this);
        }
        /// <summary>
        /// 断开连接时激活断开连接事件
        /// </summary>
        protected void OnListenerFail()
        {
            ListenerFailEvent?.Invoke(this);
        }

        public void Register(BaseToken token)
        {
            token.ExceptionEvent += OnException;
            token.LogEvent += OnLog;
        }

        public void UnRegister(BaseToken token)
        {
            token.ExceptionEvent -= OnException;
            token.LogEvent -= OnLog;
        }
    }
}
