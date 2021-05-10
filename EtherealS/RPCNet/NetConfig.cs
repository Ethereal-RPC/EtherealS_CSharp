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
        public delegate void OnLogDelegate(RPCLog log);
        public delegate void OnExceptionDelegate(Exception exception);
        #endregion

        #region --事件--
        /// <summary>
        /// 网络级拦截器事件
        /// </summary>
        public event InterceptorDelegate InterceptorEvent;
        public event OnLogDelegate LogEvent;
        #endregion

        #region --字段--
        
        #endregion

        #region --属性--
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent;
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
        internal void OnException(RPCException.ErrorCode code, string message)
        {
            OnException(new RPCException(code, message));
        }
        internal void OnException(Exception e)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e);
            }
            throw e;
        }

        internal void OnLog(RPCLog.LogCode code, string message)
        {
            OnLog(new RPCLog(code, message));
        }
        internal void OnLog(RPCLog log)
        {
            if (LogEvent != null)
            {
                LogEvent(log);
            }
        }
        #endregion
    }
}
