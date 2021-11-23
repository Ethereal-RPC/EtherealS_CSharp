using EtherealS.Core.BaseCore;
using EtherealS.Server.Interface;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace EtherealS.Server.Abstract
{
    public abstract class Server : BaseCore,IServer
    {

        #region --委托--

        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="token"></param>
        public delegate void ListenerSuccessDelegate(Server listener);
        /// <summary>
        ///     
        /// </summary>
        /// <param name="token"></param>
        public delegate void ListenerFailDelegate(Server listener);
        #endregion

        #region --事件字段--

        #endregion

        #region --事件属性--

        /// <summary>
        /// 连接事件
        /// </summary>
        public event ListenerSuccessDelegate ListenerSuccessEvent;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event ListenerFailDelegate ListenerFailEvent;
        #endregion

        #region --字段--
        protected Net.Abstract.Net net;
        protected ServerConfig config;
        protected HttpListener listener;
        protected CancellationToken cancellationToken = CancellationToken.None;
        protected List<string> prefixes = new List<string>();
        #endregion

        #region --属性--
        public HttpListener Listener { get => listener; set => listener = value; }
        public List<string> Prefixes { get => prefixes; set => prefixes = value; }
        public ServerConfig Config { get => config; set => config = value; }
        public Net.Abstract.Net Net { get => net; set => net = value; } 

        #endregion

        public abstract void Start();
        public abstract void Close();

        public Server()
        {

        }
        public Server(List<string> prefixes)
        {
            this.prefixes = prefixes;
        }
        /// <summary>
        /// 连接时激活连接事件
        /// </summary>
        protected void OnListenerSuccess()
        {
            ListenerSuccessEvent?.Invoke(this);
        }
        /// <summary>
        /// 断开连接时激活断开连接事件
        /// </summary>
        protected void OnListenerFail()
        {
            ListenerFailEvent?.Invoke(this);
        }
    }
}
