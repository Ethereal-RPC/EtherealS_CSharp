using Newtonsoft.Json;

namespace EtherealS.Model
{
    public class ServerRequestModel
    {
        private string jsonRpc;
        private string methodId;
        private string[] @params;
        private string service;

        public string JsonRpc { get => jsonRpc; set => jsonRpc = value; }
        public string MethodId { get => methodId; set => methodId = value; }
        public string[] Params { get => @params; set => @params = value; }
        public string Service { get => service; set => service = value; }

        public ServerRequestModel(string jsonrpc,string service,string methodid, string[] @params)
        {
            JsonRpc = jsonrpc;
            MethodId = methodid;
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
