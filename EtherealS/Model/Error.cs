namespace EtherealS.Model
{
    /// <summary>
    /// 网络传输错误类
    /// </summary>
    public class Error
    {
        public enum ErrorCode { Intercepted,NotFoundService,NotFoundMethod }
        /// <summary>
        /// 错误代码
        /// </summary>
        public ErrorCode Code { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 绑定数据
        /// </summary>
        public string Data { get; set; }

        public Error(ErrorCode code, string message, string data)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public override string ToString()
        {
            return "Code:" + Code 
                + " Message:" + Message 
                + " Data:" + Data + "\n";
        }
    }
}
