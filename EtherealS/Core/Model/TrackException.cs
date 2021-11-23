using System;

namespace EtherealS.Core.Model
{
    /// <summary>
    /// Ethereal-RPC异常类
    /// </summary>
    public class TrackException : Exception
    {
        #region --字段--
        public enum ErrorCode { Core, Runtime, NotEthereal }
        /// <summary>
        /// 错误代码
        /// </summary>
        private ErrorCode errorCode;
        private object sender;
        private Exception exception;
        #endregion

        #region --属性--

        public ErrorCode Error { get => errorCode; set => errorCode = value; }
        public Exception Exception { get => exception; set => exception = value; }
        public object Sender { get => sender; set => sender = value; }


        #endregion

        public TrackException(string message) : base(message)
        {
            exception = this;
        }
        public TrackException(Exception e) : base("外部库发生异常\n" + e.Message)
        {
            exception = e;
            errorCode = ErrorCode.NotEthereal;
        }

        public TrackException(ErrorCode errorCode, string message) : base(message)
        {
            exception = this;
            this.errorCode = errorCode;
        }
    }
}
