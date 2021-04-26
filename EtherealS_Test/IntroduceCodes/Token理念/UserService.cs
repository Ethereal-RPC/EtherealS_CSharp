using EtherealS.Annotation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS_Test.IntroduceCodes
{
    public class UserService
    {
        /*用户登录*/
        [RPCService]
        public bool Login(User user, string username, string password)
        {
            if (user.IsLogin == false && TryLogin(username, password))
            {
                user.IsLogin = true;			  //将Player（Token）置为已登录
                user.LoginTime = DateTime.Now();//将登录时间做记录
                return true;
            }
            return false;
        }
        /*用户获取登录时间*/
        [RPCService]
        public DateTime GetLoginTime(User player)
        {
            if (player.IsLogin)
            {
                return player.LoginTime;
            }
            return null;
        }
    }
}
