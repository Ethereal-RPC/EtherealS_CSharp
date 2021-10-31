using System.Collections.Generic;
using System.Net;
using System.Threading;
using EtherealS.Core.Delegates;
using EtherealS.Core.Model;
using EtherealS.Server.Interface;

namespace EtherealS.Server.Abstract
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

        /// <summary>
        /// BaseUserToken实例化方法委托
        /// </summary>
        /// <returns>BaseUserToken实例</returns>
        public delegate Token CreateInstance();
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

        /// <summary>
        /// 创建实例化方法委托实现
        /// </summary>
        protected CreateInstance createMethod;

        #endregion

        #region --属性--
        public CreateInstance CreateMethod { get => createMethod; set => createMethod = value; }
        public HttpListener Listener { get => listener; set => listener = value; }
        public List<string> Prefixes { get => prefixes; set => prefixes = value; }
        public ServerConfig Config { get => config; set => config = value; }
        public string NetName { get => netName; set => netName = value; }
        #endregion

        public abstract void Start();
        public abstract void Close();

        public Server(List<string> prefixes,CreateInstance createMethod)
        {
            this.prefixes = prefixes;
            this.createMethod = createMethod;
        }

        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            if (exceptionEvent != null)
            {
                e.Server = this;
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
                log.Server = this;
                logEvent?.Invoke(log);
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
        ~Server(){
            
        }
    }
}
