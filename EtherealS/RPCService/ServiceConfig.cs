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
        public delegate void OnExceptionDelegate(Exception exception);
        public delegate void OnLogDelegate(RPCLog log);
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
        internal void OnException(RPCException.ErrorCode code,string message)
        {
            OnException(new RPCException(code, message));
        }
        internal void OnException(Exception e)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e);
            }
        }

        internal void OnLog(RPCLog.LogCode code,string message)
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
