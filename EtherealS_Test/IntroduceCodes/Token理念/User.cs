using EtherealS.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS_Test.IntroduceCodes
{
    public class User : BaseUserToken
    {
        #region --字段--
        private long id;            //Token的唯一确定性凭据
        private DateTime loginTime; //登录时间
        private bool isLogin;       //是否登录
        #endregion

        #region --属性--

        public override object Key { get => id; set => id = (long)value; }
        public DateTime LoginTime { get => loginTime; set => loginTime = value; } 
        public bool IsLogin { get => isLogin; set => isLogin = value; }

        #endregion
    }
}
