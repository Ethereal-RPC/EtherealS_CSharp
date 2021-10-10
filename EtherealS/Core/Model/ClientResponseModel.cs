using System;
using System.Collections.Generic;

namespace EtherealS.Core.Model
{
    /// <summary>
    /// 客户端请求返回模型
    /// </summary>
    public class ClientResponseModel
    {
        /// <summary>
        /// Ethereal-RPC 版本
        /// </summary>
        private string type = "ER-1.0-ClientResponse";
        /// <summary>
        /// 结果值
        /// </summary>
        private string result;
        /// <summary>
        /// 错误信息
        /// </summary>
        private Error error = null;
        /// <summary>
        /// 请求ID
        /// </summary>
        private string id = null;
        /// <summary>
        /// 请求服务
        /// </summary>
        private string service;

        public string Type { get => type; set => type = value; }
        public string Result { get => result; set => result = value; }
        public Error Error { get => error; set => error = value; }
        public string Id { get => id; set => id = value; }
        public string Service { get => service; set => service = value; }

        public ClientResponseModel(string result,string id, string service, Error error)
        {
            Result = result;
            Error = error;
            Id = id;
            Service = service;
        }
        public override string ToString()
        {
            return "ClientResponseModel{" +
                    "type='" + type + '\'' +
                    ", result='" + result + '\'' +
                    ", error=" + error +
                    ", id='" + id + '\'' +
                    ", service='" + service + '\'' +
                    '}';
        }
    }
}
