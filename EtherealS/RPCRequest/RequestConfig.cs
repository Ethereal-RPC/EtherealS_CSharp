using EtherealS.Model;
using System;

namespace EtherealS.RPCRequest
{
    /// <summary>
    /// 服务请求配置项
    /// </summary>
    public class RequestConfig
    {
        #region --字段--
        /// <summary>
        /// 中间层抽象数据类配置项
        /// </summary>
        private RPCTypeConfig types;


        #endregion

        #region --委托--
        public delegate void OnExceptionDelegate(Exception exception);
        public delegate void OnLogDelegate(RPCLog log);
        #endregion

        #region --事件--
        public event OnLogDelegate LogEvent;
        #endregion

        #region --属性--
        public RPCTypeConfig Types { get => types; set => types = value; }

        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent;
        #endregion

        #region --方法--
        public RequestConfig(RPCTypeConfig type)
        {
            this.types = type;
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
