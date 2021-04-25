using EtherealS.Model;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System.Text;

namespace EtherealS.NativeServer
{
    public class ServerConfig
    {
        #region --委托--
        public delegate BaseUserToken CreateInstance();
        public delegate string ServerRequestModelSerializeDelegate(ServerRequestModel obj);
        public delegate ClientRequestModel ClientRequestModelDeserializeDelegate(string obj);
        public delegate string ClientResponseModelSerializeDelegate(ClientResponseModel obj);
        #endregion

        #region --字段--
        private int numConnections = 1024;
        private int bufferSize = 1024;
        private int maxBufferSize = 10240;
        private int numChannels = 5;
        private bool autoManageTokens = true;
        private CreateInstance createMethod;
        private Encoding encoding = Encoding.UTF8;
        private int dynamicAdjustBufferCount = 1;
        private ServerRequestModelSerializeDelegate serverRequestModelSerialize;
        private ClientRequestModelDeserializeDelegate clientRequestModelDeserialize;
        private ClientResponseModelSerializeDelegate clientResponseModelSerialize;
        #endregion

        #region --属性--
        public CreateInstance CreateMethod { get => createMethod; set => createMethod = value; }
        public bool AutoManageTokens { get => autoManageTokens; set => autoManageTokens = value; }
        public int NumConnections { get => numConnections; set => numConnections = value; }
        public int BufferSize { get => bufferSize; set => bufferSize = value; }
        public int NumChannels { get => numChannels; set => numChannels = value; }
        public Encoding Encoding { get => encoding; set => encoding = value; }
        public int MaxBufferSize { get => maxBufferSize; set => maxBufferSize = value; }
        public int DynamicAdjustBufferCount { get => dynamicAdjustBufferCount; set => dynamicAdjustBufferCount = value; }
        public ServerRequestModelSerializeDelegate ServerRequestModelSerialize { get => serverRequestModelSerialize; set => serverRequestModelSerialize = value; }
        public ClientRequestModelDeserializeDelegate ClientRequestModelDeserialize { get => clientRequestModelDeserialize; set => clientRequestModelDeserialize = value; }
        public ClientResponseModelSerializeDelegate ClientResponseModelSerialize { get => clientResponseModelSerialize; set => clientResponseModelSerialize = value; }
        #endregion

        public ServerConfig(CreateInstance createMethod)
        {
            this.createMethod = createMethod;
            serverRequestModelSerialize = (obj)=>JsonConvert.SerializeObject(obj);
            clientResponseModelSerialize = (obj) => JsonConvert.SerializeObject(obj);
            clientRequestModelDeserialize = (obj) => JsonConvert.DeserializeObject<ClientRequestModel>(obj);
        }
    }
}
