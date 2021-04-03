using EtherealS.Model;

namespace EtherealS.NativeNetwork
{
    public class ServerConfig
    {
        #region --字段--
        private int numConnections = 1024;
        private int bufferSize = 1024;
        private int numChannels = 5;
        private bool autoManageTokens = true;
        private BaseUserToken.CreateInstance createMethod;
        #endregion

        #region --属性--
        public BaseUserToken.CreateInstance CreateMethod { get => createMethod; set => createMethod = value; }
        public bool AutoManageTokens { get => autoManageTokens; set => autoManageTokens = value; }
        public int NumConnections { get => numConnections; set => numConnections = value; }
        public int BufferSize { get => bufferSize; set => bufferSize = value; }
        public int NumChannels { get => numChannels; set => numChannels = value; }
        #endregion

        public ServerConfig(BaseUserToken.CreateInstance createMethod)
        {
            this.createMethod = createMethod;
        }
    }
}
