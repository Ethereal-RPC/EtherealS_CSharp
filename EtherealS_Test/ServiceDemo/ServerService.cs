using EtherealS.Core.Attribute;
using EtherealS.Server.Abstract;
using EtherealS.Server.Attribute;
using EtherealS.Service.Attribute;
using EtherealS_Test.Model;
using EtherealS_Test.RequestDemo;

namespace EtherealS_Test.ServiceDemo
{
    public class ServerService : EtherealS.Service.WebSocket.WebSocketService
    {
        #region --字段--
        /// <summary>
        /// 服务端向客户端发送请求的接口
        /// </summary>
        private IClientRequest userRequest;
        #endregion

        #region --属性--
        public IClientRequest UserRequest { get => userRequest; set => userRequest = value; }
        #endregion

        #region --方法--
        //Token 
        [Service]
        public bool Register([EtherealS.Server.Attribute.Token]User user, string username, long id)
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
        public bool SendSay([EtherealS.Server.Attribute.Token] User sender, long listener_id, string message)
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
        public int Add([EtherealS.Server.Attribute.Token] EtherealS.Server.Abstract.Token token,int a,int b)
        {
            return a + b;
        }

        public override void Initialization()
        {
            
        }

        public override void UnInitialization()
        {
            
        }

        #endregion

    }
}
