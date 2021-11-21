using Newtonsoft.Json;
using System.Threading;

namespace EtherealS.Core.Model
{
    public class ClientRequestModel
    {
        [JsonIgnore]
        private ClientResponseModel result;
        private string type = "ER-1.0-ClientRequest";
        private string mapping;
        private string[] @params;
        private string id;
        [JsonIgnore]
        private AutoResetEvent sign = new AutoResetEvent(false);
        public ClientResponseModel Result { get => result; set => result = value; }
        public string Type { get => type; set => type = value; }
        public string Mapping { get => mapping; set => mapping = value; }
        public string[] Params { get => @params; set => @params = value; }
        public string Id { get => id; set => id = value; }
        public AutoResetEvent Sign { get => sign; set => sign = value; }


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
                    ", methodId='" + mapping + '\'' +
                    ", params=" + string.Join(",", @params) +
                    ", id='" + id + '\'' +
                    '}';
        }
    }
}
