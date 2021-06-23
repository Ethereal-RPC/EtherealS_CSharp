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
        public delegate void OnExceptionDelegate(Exception exception,Request request);
        public delegate void OnLogDelegate(RPCLog log,Request request);
        #endregion

        #region --事件--
        public event OnLogDelegate LogEvent;
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent;
        #endregion

        #region --属性--
        public RPCTypeConfig Types { get => types; set => types = value; }


        #endregion

        #region --方法--
        public RequestConfig(RPCTypeConfig type)
        {
            this.types = type;
        }
        internal void OnException(RPCException.ErrorCode code, string message, Request request)
        {
            OnException(new RPCException(code, message),request);
        }
        internal void OnException(Exception e, Request request)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e,request);
            }
            throw e;
        }

        internal void OnLog(RPCLog.LogCode code, string message, Request request)
        {
            OnLog(new RPCLog(code, message), request);
        }
        internal void OnLog(RPCLog log, Request request)
        {
            if (LogEvent != null)
            {
                LogEvent(log, request);
            }
        }
        #endregion
    }
}
