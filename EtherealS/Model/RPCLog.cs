using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS.Model
{
    public class RPCLog
    {
        public enum LogCode { Register, Runtime }

        #region --字段--
        private string message;
        private LogCode code;
        #endregion


        #region --属性--
        public string Message { get => message; set => message = value; }
        public LogCode Code { get => code; set => code = value; }
        #endregion

        public RPCLog(LogCode code,string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}
