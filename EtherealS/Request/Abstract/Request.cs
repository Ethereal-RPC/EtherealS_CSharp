using System;
using System.Reflection;
using EtherealS.Core.Delegates;
using EtherealS.Core.Model;
using EtherealS.Request.Interface;

namespace EtherealS.Request.Abstract
{
    public abstract class Request : DispatchProxy,IRequest
    {

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
        #endregion

        #region --字段--
        protected string name;
        protected string netName;
        protected RequestConfig config;
        protected AbstractTypes types = new AbstractTypes();
        #endregion

        #region --属性--
        public string Name { get => name; set => name = value; }
        public RequestConfig Config { get => config; set => config = value; }
        public string NetName { get => netName; set => netName = value; }
        public AbstractTypes Types { get => types; set => types = value; }
        #endregion

        public static R Register<R,T>() where R:Request
        {
            R proxy = Create<T, R>() as R;
            return proxy;
        }
        protected override abstract object Invoke(MethodInfo targetMethod, object[] args);

        public void OnException(TrackException.ErrorCode code, string message)
        {
            OnException(new TrackException(code, message));
        }
        public void OnException(TrackException e)
        {
            if (exceptionEvent != null)
            {
                e.Request = this;
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
                log.Request = this;
                logEvent?.Invoke(log);
            }
        }

        public abstract void Initialize();
        public abstract void UnInitialize();
    }
}
