using EtherealS.Model;
using EtherealS.RPCNet;
using System.Text;

namespace EtherealS.NativeServer
{
    public class ServerConfig
    {
        #region --委托--
        public delegate BaseUserToken CreateInstance();
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
        #endregion

        public ServerConfig(CreateInstance createMethod)
        {
            this.createMethod = createMethod;
        }
    }
}
