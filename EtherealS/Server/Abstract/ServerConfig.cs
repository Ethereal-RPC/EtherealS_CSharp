using EtherealS.Core.Model;
using EtherealS.Server.Interface;
using Newtonsoft.Json;
using System;

namespace EtherealS.Server.Abstract
{
    /// <summary>
    /// Server配置项
    /// </summary>
    public abstract class ServerConfig:IServerConfig
    {
        #region --委托--

        #endregion

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
        /// 最大WebSocket缓冲池
        /// </summary>
        private int maxBufferSize = 10240;
        /// <summary>
        /// 多行道[C# 行道为5行道并行接收请求和处理请求（待更改）]
        /// </summary>
        private int numChannels = 5;
        /// <summary>
        /// 心跳周期
        /// </summary>
        private TimeSpan keepAliveInterval = TimeSpan.FromSeconds(60);
        #endregion

        #region --属性--
        public int BufferSize { get => bufferSize; set => bufferSize = value; }
        public int NumChannels { get => numChannels; set => numChannels = value; }
        public int MaxConnections { get => maxConnections; set => maxConnections = value; }
        public int MaxBufferSize { get => maxBufferSize; set => maxBufferSize = value; }
        public TimeSpan KeepAliveInterval { get => keepAliveInterval; set => keepAliveInterval = value; }

        #endregion

        #region --方法--
        public ServerConfig()
        {

        }

        #endregion
    }
}
