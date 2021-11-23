namespace EtherealS.Core.Model
{
    public class TrackLog
    {
        public enum LogCode { Core, Runtime }

        #region --字段--
        private string message;
        private LogCode code;
        private object sender;
        #endregion

        #region --属性--
        public string Message { get => message; set => message = value; }
        public LogCode Code { get => code; set => code = value; }
        public object Sender { get => sender; set => sender = value; }

        #endregion

        public TrackLog(LogCode code, string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}
