namespace EtherealS.Core.Model
{
    public class ServerRequestModel
    {
        /// <summary>
        /// Ethereal-RPC 版本
        /// </summary>
        private string type = "ER-1.0-ServerRequest";
        /// <summary>
        /// 方法ID
        /// </summary>
        private string mapping;
        /// <summary>
        /// 方法参数
        /// </summary>
        private string[] @params;
        /// <summary>
        /// 请求服务
        /// </summary>
        private string service;

        public string Type { get => type; set => type = value; }
        public string Mapping { get => mapping; set => mapping = value; }
        public string[] Params { get => @params; set => @params = value; }
        public string Service { get => service; set => service = value; }
        public override string ToString()
        {
            return "ServerRequestModel{" +
                    "type='" + type + '\'' +
                    ", methodId='" + mapping + '\'' +
                    ", params=" + string.Join(",", @params) +
                    ", service='" + service + '\'' +
                    '}';
        }
    }
}
