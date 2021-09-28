using System;
using EtherealS_Test.Model;
using EtherealS_Test.RequestDemo;
using System.Threading;
using System.Threading.Tasks;
using EtherealS.Server.Abstract;
using EtherealS.Service.Attribute;
using ServiceConfig = EtherealS.Service.Abstract.ServiceConfig;

namespace EtherealS_Test.ServiceDemo
{
    public class ServerService : EtherealS.Service.WebSocket.WebSocketService
    {
        #region --字段--
        /// <summary>
        /// 服务端向客户端发送请求的接口
        /// </summary>
        private IClientRequest userRequest;
        [EtherealS.Service.Attribute.ServiceConfig]
        public ServiceConfig config;
        #endregion

        #region --属性--
        public IClientRequest UserRequest { get => userRequest; set => userRequest = value; }
        #endregion

        #region --方法--
        //Token 
        [Service]
        public bool Register(User user, string username, long id)
        {
            user.Username = username;
            user.Id = id;
            return user.Register();
        }
        /// <summary>
        /// 接受客户端发送来的命令请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="listener_id"></param>
        /// <param name="message"></param>
        /// <returns></returns>

        [Service]
        public bool SendSay(User sender, long listener_id, string message)
        {
            //查找对应ID的用户 1
            if (sender.GetToken(listener_id, out User listener))
            {
                //向listener用户发送Hello请求
                userRequest.Say(listener, sender, message);
                return true;
            }
            else return false;
        }

        [Service]
        public int Add(BaseToken token,int a,int b)
        {
            return a + b;
        }

        #endregion

    }
}
