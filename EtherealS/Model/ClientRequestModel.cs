using Newtonsoft.Json;

namespace EtherealS.Model
{
    /// <summary>
    /// 客户端请求模型
    /// </summary>
    public class ClientRequestModel
    {
        /// <summary>
        /// Ethereal-RPC 版本
        /// </summary>
        private string jsonRpc;
        /// <summary>
        /// 方法ID
        /// </summary>
        private string methodId;
        /// <summary>
        /// 方法参数
        /// </summary>
        private object[] @params;
        /// <summary>
        /// 请求ID
        /// </summary>
        private string id;
        /// <summary>
        /// 请求服务
        /// </summary>
        private string service;

        public string JsonRpc { get => jsonRpc; set => jsonRpc = value; }
        public string MethodId { get => methodId; set => methodId = value; }
        public object[] Params { get => @params; set => @params = value; }
        public string Id { get => id; set => id = value; }
        public string Service { get => service; set => service = value; }

        public ClientRequestModel(string jsonRpc, string service, string methodId, object[] @params)
        {
            JsonRpc = jsonRpc;
            MethodId = methodId;
            Params = @params;
            Service = service;
        }
        public override string ToString()
        {
            return "Jsonrpc:" + JsonRpc + "\n"
                + "Service:" + Service + "\n"
                + "Methodid:" + MethodId + "\n"
                + "Params:" + JsonConvert.SerializeObject(Params);
        }
    }
}
