using EtherealS.Model;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System;
using System.Text;

namespace EtherealS.NativeServer
{
    /// <summary>
    /// Server配置项
    /// </summary>
    public class ServerConfig
    {
        #region --委托--
        /// <summary>
        /// BaseUserToken实例化方法委托
        /// </summary>
        /// <returns>BaseUserToken实例</returns>
        public delegate BaseUserToken CreateInstance();
        /// <summary>
        /// ServerRequestModel序列化方法委托
        /// </summary>
        /// <param name="obj">待序列化ServerRequestModel对象</param>
        /// <returns>序列化文本</returns>
        public delegate string ServerRequestModelSerializeDelegate(ServerRequestModel obj);
        /// <summary>
        /// ClientRequestModel逆序列化方法委托
        /// </summary>
        /// <param name="obj">序列化文本</param>
        /// <returns>序列化结果类</returns>
        public delegate ClientRequestModel ClientRequestModelDeserializeDelegate(string obj);
        /// <summary>
        /// ClientResponseModel序列化方法委托
        /// </summary>
        /// <param name="obj">待序列化ClientResponseModel对象</param>
        /// <returns>序列化文本</returns>
        public delegate string ClientResponseModelSerializeDelegate(ClientResponseModel obj);

        public delegate void OnExceptionDelegate(Exception exception,ServerListener server);

        public delegate void OnLogDelegate(RPCLog log,ServerListener server);
        #endregion

        #region --事件--
        public event OnLogDelegate LogEvent;
        /// <summary>
        /// 抛出异常事件
        /// </summary>
        public event OnExceptionDelegate ExceptionEvent;
        #endregion

        #region --字段--
        /// <summary>
        /// 最大的连接数
        /// </summary>
        private int numConnections = 1024;
        /// <summary>
        /// 默认缓冲池
        /// </summary>
        private int bufferSize = 1024;
        /// <summary>
        /// 允许最大存储池
        /// </summary>
        private int maxBufferSize = 10240;
        /// <summary>
        /// 多行道[C# 行道为5行道并行接收请求和处理请求（待更改）]
        /// </summary>
        private int numChannels = 5;
        /// <summary>
        /// 自动管理Token
        /// </summary>
        private bool autoManageTokens = true;
        /// <summary>
        /// 创建实例化方法委托实现
        /// </summary>
        private CreateInstance createMethod;
        /// <summary>
        /// 编码类型
        /// </summary>
        private Encoding encoding = Encoding.UTF8;
        /// <summary>
        /// 动态调整Buffer区的初始次数（减为0时进行回收）
        /// </summary>
        private uint dynamicAdjustBufferCount = 1;
        /// <summary>
        /// ServerRequestModel序列化委托实现
        /// </summary>
        private ServerRequestModelSerializeDelegate serverRequestModelSerialize;
        /// <summary>
        /// ClientRequestModel逆序列化委托实现
        /// </summary>
        private ClientRequestModelDeserializeDelegate clientRequestModelDeserialize;
        /// <summary>
        /// ClientResponseModel序列化委托实现
        /// </summary>
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
        public uint DynamicAdjustBufferCount { get => dynamicAdjustBufferCount; set => dynamicAdjustBufferCount = value; }
        public ServerRequestModelSerializeDelegate ServerRequestModelSerialize { get => serverRequestModelSerialize; set => serverRequestModelSerialize = value; }
        public ClientRequestModelDeserializeDelegate ClientRequestModelDeserialize { get => clientRequestModelDeserialize; set => clientRequestModelDeserialize = value; }
        public ClientResponseModelSerializeDelegate ClientResponseModelSerialize { get => clientResponseModelSerialize; set => clientResponseModelSerialize = value; }

        #endregion

        #region --方法--
        public ServerConfig(CreateInstance createMethod)
        {
            this.createMethod = createMethod;
            serverRequestModelSerialize = (obj) => JsonConvert.SerializeObject(obj);
            clientResponseModelSerialize = (obj) => JsonConvert.SerializeObject(obj);
            clientRequestModelDeserialize = (obj) => JsonConvert.DeserializeObject<ClientRequestModel>(obj);
        }
        internal void OnException(RPCException.ErrorCode code, string message,ServerListener server)
        {
            OnException(new RPCException(code, message),server);
        }
        internal void OnException(Exception e, ServerListener server)
        {
            if (ExceptionEvent != null)
            {
                ExceptionEvent(e,server);
            }
            throw e;
        }

        internal void OnLog(RPCLog.LogCode code, string message, ServerListener server)
        {
            OnLog(new RPCLog(code, message),server);
        }
        internal void OnLog(RPCLog log, ServerListener server)
        {
            if (LogEvent != null)
            {
                LogEvent(log,server);
            }
        }
        #endregion
    }
}
