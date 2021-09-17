﻿namespace EtherealS.Core.Model
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
        private object result = null;
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
        /// <summary>
        /// 结果值类型
        /// </summary>
        private string resultType;

        public string Type { get => type; set => type = value; }
        public object Result { get => result; set => result = value; }
        public Error Error { get => error; set => error = value; }
        public string Id { get => id; set => id = value; }
        public string ResultType { get => resultType; set => resultType = value; }
        public string Service { get => service; set => service = value; }

        public ClientResponseModel(object result, string resultType, string id, string service, Error error)
        {
            Result = result;
            Error = error;
            Id = id;
            ResultType = resultType;
            Service = service;
        }
        public override string ToString()
        {

            return "Type:" + Type + "\n"
                + "Id:" + Id + "\n"
                + "Result:" + Result + "\n";
        }
    }
}