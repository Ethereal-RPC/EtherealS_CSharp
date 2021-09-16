using EtherealS.Core.Model;
using EtherealS.NativeServer;

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

        private string key;
        #endregion


        #region --属性--
        public long Id { get => id; set => id = value; }
        public string Username { get => username; set => username = value; }
        public override object Key { get => key; set => key = (string)value; }

        public override void DisConnect(string reason)
        {
            throw new System.NotImplementedException();
        }
        #endregion

    }
}
