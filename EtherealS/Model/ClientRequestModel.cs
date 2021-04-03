using Newtonsoft.Json;

namespace EtherealS.Model
{
    public class ClientRequestModel
    {
        private string jsonRpc;
        private string methodId;
        private object[] @params;
        private string id;
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
