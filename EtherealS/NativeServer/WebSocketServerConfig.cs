using EtherealS.Core.Model;
using EtherealS.RPCNet;
using Newtonsoft.Json;
using System;
using System.Text;
using static EtherealS.NativeServer.ServerConfig;

namespace EtherealS.NativeServer
{
    /// <summary>
    /// Server配置项
    /// </summary>
    public class WebSocketServerConfig:ServerConfig
    {
        #region --字段--
        /// <summary>
        /// 最大的连接数
        /// </summary>
        private int maxConnections = 1024;
        /// <summary>
        /// 默认缓冲池
        /// </summary>
        private int bufferSize = 1024;        
        /// <summary>
        /// 最大缓冲池
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
        /// 编码类型
        /// </summary>
        private Encoding encoding = Encoding.UTF8;

        /// <summary>
        /// 心跳周期
        /// </summary>
        private TimeSpan keepAliveInterval = TimeSpan.FromSeconds(60);
        #endregion

        #region --属性--
        public bool AutoManageTokens { get => autoManageTokens; set => autoManageTokens = value; }
        public int BufferSize { get => bufferSize; set => bufferSize = value; }
        public int NumChannels { get => numChannels; set => numChannels = value; }
        public Encoding Encoding { get => encoding; set => encoding = value; }
        public int MaxConnections { get => maxConnections; set => maxConnections = value; }
        public int MaxBufferSize { get => maxBufferSize; set => maxBufferSize = value; }
        public TimeSpan KeepAliveInterval { get => keepAliveInterval; set => keepAliveInterval = value; }

        #endregion

        #region --方法--
        public WebSocketServerConfig(CreateInstance createMethod):base(createMethod)
        {

        }

        #endregion
    }
}
