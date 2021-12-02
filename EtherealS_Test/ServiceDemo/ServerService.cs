using EtherealS.Core.Manager.AbstractType;
using EtherealS.Core.Manager.Event.Attribute;
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
        [ServiceMapping(Mapping: "Register")]
        public bool Register([TokenParam] User user, string username, long id)
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

        [ServiceMapping(Mapping: "SendSay")]
        public bool SendSay([TokenParam] User sender, long listener_id, string message)
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

        [ServiceMapping(Mapping: "Add")]
        public int Add([TokenParam] User token, int a, int b)
        {
            token.Username = "asd";
            userRequest.Say(token, token, token.Username);
            return a + b;
        }
        [ServiceMapping(Mapping: "test")]
        [AfterEventAttribute("instance.after(ddd:d,s:s)")]
        public bool Test([TokenParam] User token, int d, string s, int k)
        {
            Console.WriteLine($"token:{token} d:{d} s:{s} k:{k}");
            return true;
        }

        protected override void Initialize()
        {
            Name = "Server";
            Types.Add<int>("Int");
            Types.Add<int>("Int1");
            Types.Add<User>("User");
            Types.Add<long>("Long");
            Types.Add<string>("String");
            Types.Add<bool>("Bool");
            TokenCreateInstance = () => new User();
        }

        protected override void UnInitialize()
        {

        }

        protected override void Register()
        {
            object instance = new EventClass();
            IOCManager.Register("instance", instance);
        }

        protected override void UnRegister()
        {

        }

        #endregion

    }
}
