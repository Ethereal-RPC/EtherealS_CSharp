namespace EtherealS.Core.Model
{
    /// <summary>
    /// 客户端请求返回模型
    /// </summary>
    public class ClientResponseModel
    {
        /// <summary>
        /// Ethereal-RPC 版本
        /// </summary>
        private string type = "ER-1.0-ClientResponse";
        /// <summary>
        /// 结果值
        /// </summary>
        private string result;
        /// <summary>
        /// 错误信息
        /// </summary>
        private Error error = null;
        /// <summary>
        /// 请求ID
        /// </summary>
        private string id = null;
        public ClientResponseModel()
        {

        }
        public ClientResponseModel(string id,string result, Error error)
        {
            Result = result;
            Error = error;
            Id = id;
        }

        public string Type { get => type; set => type = value; }
        public string Result { get => result; set => result = value; }
        public Error Error { get => error; set => error = value; }
        public string Id { get => id; set => id = value; }



        public override string ToString()
        {
            return "ClientResponseModel{" +
                    "type='" + type + '\'' +
                    ", result='" + result + '\'' +
                    ", error=" + error +
                    ", id='" + id + '\'' +
                    '}';
        }
    }
}
