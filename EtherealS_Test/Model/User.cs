using EtherealS.Core.Model;
using EtherealS.Server.WebSocket;

namespace EtherealS_Test.Model
{
    public class User:WebSocketToken
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
        public long Id { get => id; set => id = value; }
        public string Username { get => username; set => username = value; }
        public object Key { get => key; set => key = (string)value; }
        #endregion

    }
}
