using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

namespace EtherealS.Core.Model
{
    public class ClientRequestModel
    {
        [JsonIgnore]
        private ClientResponseModel result;
        private string type = "ER-1.0-ClientRequest";
        private string methodId;
        private object[] @params;
        private string id;
        private string service;
        [JsonIgnore]
        private AutoResetEvent sign = new AutoResetEvent(false);
        public ClientResponseModel Result { get => result; set => result = value; } 
        public string Type { get => type; set => type = value; }
        public string MethodId { get => methodId; set => methodId = value; }
        public object[] Params { get => @params; set => @params = value; }
        public string Id { get => id; set => id = value; }
        public string Service { get => service; set => service = value; }
        public AutoResetEvent Sign { get => sign; set => sign = value; }

        public ClientRequestModel(string service,string methodId, object[] @params)
        {
            MethodId = methodId;
            Params = @params;
            Service = service;
        }

        public void Set(ClientResponseModel result)
        {
            Result = result;
            Sign.Set();
        }
        public ClientResponseModel Get(int timeout)
        {
            //暂停当前进程，等待返回.
            while (Result == null)
            {
                if (timeout == -1) Sign.WaitOne();
                else Sign.WaitOne(timeout);
            }
            return Result;
        }

        public override string ToString()
        {
            return "ClientRequestModel{" +
                    "result=" + result +
                    ", type='" + type + '\'' +
                    ", methodId='" + methodId + '\'' +
                    ", params=" + string.Join("参数：",@params) +
                    ", id='" + id + '\'' +
                    ", service='" + service + '\'' +
                    '}';
        }
    }
}
