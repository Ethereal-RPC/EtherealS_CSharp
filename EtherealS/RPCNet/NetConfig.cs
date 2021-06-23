using System;
using System.Collections.Concurrent;
using System.Reflection;
using EtherealS.Model;
using EtherealS.RPCService;

namespace EtherealS.RPCNet
{
    /// <summary>
    /// Ethereal网关
    /// </summary>
    public class NetConfig
    {
        #region --委托--
        public delegate bool InterceptorDelegate(Service service,MethodInfo method,BaseUserToken token);
        public delegate void OnLogDelegate(RPCLog log,Net net);
        public delegate void OnExceptionDelegate(Exception exception,Net net);
        #endregion

        #region --事件--
        /// <summary>
        /// 网络级拦截器事件
        /// </summary>
        public event InterceptorDelegate InterceptorEvent;
        public event OnLogDelegate LogEvent;
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent;
        #endregion

        #region --字段--

        #endregion

        #region --属性--

        #endregion

        #region --方法--
        public bool OnInterceptor(Service service,MethodInfo method,BaseUserToken token)
        {
            if (InterceptorEvent != null)
            {
                foreach (InterceptorDelegate item in InterceptorEvent.GetInvocationList())
                {
                    if (!item.Invoke(service, method, token)) return false;
                }
                return true;
            }
            else return true;
        }
        internal void OnException(RPCException.ErrorCode code, string message,Net net)
        {
            OnException(new RPCException(code, message),net);
        }
        internal void OnException(Exception e,Net net)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e,net);
            }
            throw e;
        }

        internal void OnLog(RPCLog.LogCode code, string message,Net net)
        {
            OnLog(new RPCLog(code, message),net);
        }
        internal void OnLog(RPCLog log,Net net)
        {
            if (LogEvent != null)
            {
                LogEvent(log,net);
            }
        }
        #endregion
    }
}
