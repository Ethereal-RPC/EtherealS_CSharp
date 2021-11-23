using EtherealS.Service.WebSocket;
using Newtonsoft.Json;

namespace EtherealS_Test.Model
{
    public class User : WebSocketToken
    {
        #region --字段--
        /// <summary>
        /// 唯一ID
        /// </summary>
        private long id;
        /// <summary>
        /// 用户名
        /// </summary>
        private string username;
        #endregion


        #region --属性--
        [JsonProperty]
        public long Id { get => id; set => id = value; }
        [JsonProperty]
        public string Username { get => username; set => username = value; }
        [JsonProperty]
        public new object Key { get => base.Key; set => base.Key = (string)value; }
        #endregion

    }
}
