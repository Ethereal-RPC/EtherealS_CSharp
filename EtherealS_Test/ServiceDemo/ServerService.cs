using EtherealS.Core.EventManage.Attribute;
using EtherealS.Service.Attribute;
using EtherealS_Test.Model;
using EtherealS_Test.RequestDemo;
using System;

namespace EtherealS_Test.ServiceDemo
{
    public class ServerService : EtherealS.Service.WebSocket.WebSocketService
    {
        #region --字段--
        /// <summary>
        /// 服务端向客户端发送请求的接口
        /// </summary>
        private ClientRequest userRequest;
        #endregion

        #region --属性--
        public ClientRequest UserRequest { get => userRequest; set => userRequest = value; }
        #endregion

        #region --方法--
        //Token 
        [ServiceMethod(Mapping: "Register")]
        public bool Register([EtherealS.Server.Attribute.Token] User user, string username, long id)
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

        [ServiceMethod(Mapping: "SendSay")]
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

        [ServiceMethod(Mapping: "Add")]
        public int Add([EtherealS.Server.Attribute.Token] User token, int a, int b)
        {
            token.Username = "asd";
            userRequest.Say(token, token, token.Username);
            return a + b;
        }
        [AfterEvent("instance.after(ddd:d,s:s)")]
        [ServiceMethod(Mapping: "test")]
        public virtual bool test(int d, string s)
        {
            Console.WriteLine("Add");
            return true;
        }
        public ServerService()
        {
            types.Add<int>("Int");
            types.Add<User>("User");
            types.Add<long>("Long");
            types.Add<string>("String");
            types.Add<bool>("Bool");
            TokenCreateInstance = () => new User();
        }
        public override void Initialize()
        {
            object instance = new EventClass();
            RegisterIoc("instance", instance);
            EventManager.RegisterEventMethod("instance", instance);
        }

        public override void UnInitialize()
        {

        }

        #endregion

    }
}
