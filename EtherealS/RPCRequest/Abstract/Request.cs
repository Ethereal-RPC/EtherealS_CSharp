using System;
using System.Reflection;
using EtherealS.Core.Delegates;
using EtherealS.Core.Model;
using EtherealS.RPCRequest.Interface;

namespace EtherealS.RPCRequest.Abstract
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
        private string netName;
        protected RequestConfig config;
        #endregion

        #region --属性--
        public string Name { get => name; set => name = value; }
        public RequestConfig Config { get => config; set => config = value; }
        public string NetName { get => netName; set => netName = value; }
        #endregion

        public static Request Register<T>(string netName, string servicename, RequestConfig config)
        {
            Request proxy = Create<T, Request>() as Request;
            proxy.Name = servicename;
            proxy.NetName = netName ?? throw new ArgumentNullException(nameof(netName));
            proxy.Config = config;
            return proxy;
        }
        protected override abstract object Invoke(MethodInfo targetMethod, object[] args);

        public void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        public void OnException(RPCException e)
        {
            if (exceptionEvent != null)
            {
                e.Request = this;
                exceptionEvent?.Invoke(e);
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
                log.Request = this;
                logEvent?.Invoke(log);
            }
        }
    }
}
