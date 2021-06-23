using System;
using System.Reflection;
using EtherealS.Model;

namespace EtherealS.RPCService
{
    /// <summary>
    /// 服务配置项
    /// </summary>
    public class ServiceConfig
    {
        #region --委托--
        public delegate bool InterceptorDelegate(Service service, MethodInfo method, BaseUserToken token);
        public delegate void OnExceptionDelegate(Exception exception,Service service);
        public delegate void OnLogDelegate(RPCLog log,Service service);
        #endregion

        #region --事件--
        /// <summary>
        /// 拦截器事件
        /// </summary>
        public event InterceptorDelegate InterceptorEvent;
        public event OnLogDelegate LogEvent;
        #endregion

        #region --字段--
        /// <summary>
        /// 中间层抽象数据类配置项
        /// </summary>
        private RPCTypeConfig types;
        #endregion

        #region --属性--
        public RPCTypeConfig Types { get => types; set => types = value; }
        public event OnExceptionDelegate ExceptionEvent;
        #endregion

        #region --方法--
        public ServiceConfig(RPCTypeConfig type)
        {
            this.types = type;
        }

        public ServiceConfig(RPCTypeConfig type, bool tokenEnable)
        {
            this.types = type;
        }
        internal bool OnInterceptor(Service service, MethodInfo method, BaseUserToken token)
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
        internal void OnException(RPCException.ErrorCode code,string message, Service service)
        {
            OnException(new RPCException(code, message), service);
        }
        internal void OnException(Exception e,Service service)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e, service);
            }
            throw e;
        }

        internal void OnLog(RPCLog.LogCode code,string message, Service service)
        {
            OnLog(new RPCLog(code, message),service);
        }
        internal void OnLog(RPCLog log, Service service)
        {
            if (LogEvent != null)
            {
                LogEvent(log,service);
            }
        }
        #endregion
    }
}
