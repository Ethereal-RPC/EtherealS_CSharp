using EtherealS.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS_Test.Model
{
    public class User:BaseUserToken
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
        /// <summary>
        /// 继承BaseUserToken，Key作为Token池中唯一识别码
        /// </summary>
        public override object Key { get => Id; set => Id = (int)value; }
        public long Id { get => id; set => id = value; }
        public string Username { get => username; set => username = value; }
        #endregion

    }
}
