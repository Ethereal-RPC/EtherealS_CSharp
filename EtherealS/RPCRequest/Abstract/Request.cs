using EtherealS.Model;
using EtherealS.NativeServer;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text;
using static EtherealS.Core.Delegate.Delegates;

namespace EtherealS.RPCRequest
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
        #endregion

        #region --属性--
        public string Name { get => name; set => name = value; }
        public RequestConfig Config { get => config; set => config = value; }
        #endregion

        public static Request Register<T>(string netName, string servicename, RequestConfig config)
        {
            Request proxy = Create<T, Request>() as Request;
            proxy.Name = servicename;
            proxy.netName = netName ?? throw new ArgumentNullException(nameof(netName));
            proxy.Config = config;
            return proxy;
        }
        protected override abstract object Invoke(MethodInfo targetMethod, object[] args);

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
                (e as RPCException).Request = this;
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
                log.Request = this;
                logEvent(log);
            }
        }
    }
}
