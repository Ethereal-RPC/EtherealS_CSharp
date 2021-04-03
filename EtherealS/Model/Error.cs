namespace EtherealS.Model
{
    public class Error
    {
        public enum ErrorCode { Intercepted }
        public ErrorCode Code { get; set; }
        public string Message { get; set; }
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
