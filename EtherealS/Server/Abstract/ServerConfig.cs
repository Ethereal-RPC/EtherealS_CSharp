﻿using EtherealS.Core.Model;
using EtherealS.Server.Interface;
using Newtonsoft.Json;

namespace EtherealS.Server.Abstract
{
    /// <summary>
    /// Server配置项
    /// </summary>
    public abstract class ServerConfig:IServerConfig
    {
        #region --委托--
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

        #endregion

        #region --字段--
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
        public ServerRequestModelSerializeDelegate ServerRequestModelSerialize { get => serverRequestModelSerialize; set => serverRequestModelSerialize = value; }
        public ClientRequestModelDeserializeDelegate ClientRequestModelDeserialize { get => clientRequestModelDeserialize; set => clientRequestModelDeserialize = value; }
        public ClientResponseModelSerializeDelegate ClientResponseModelSerialize { get => clientResponseModelSerialize; set => clientResponseModelSerialize = value; }

        #endregion

        #region --方法--
        public ServerConfig()
        {
            serverRequestModelSerialize = (obj) => JsonConvert.SerializeObject(obj);
            clientResponseModelSerialize = (obj) => JsonConvert.SerializeObject(obj);
            clientRequestModelDeserialize = (obj) => JsonConvert.DeserializeObject<ClientRequestModel>(obj);
        }

        #endregion
    }
}
