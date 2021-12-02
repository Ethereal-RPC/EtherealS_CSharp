using EtherealS.Core;
using EtherealS.Core.BaseCore;
using EtherealS.Core.Model;
using EtherealS.Net.Extension.Plugins;
using EtherealS.Net.Interface;
using EtherealS.Service.Abstract;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace EtherealS.Net.Abstract
{
    public abstract class Net : BaseCore,INet
    {
        public enum NetType { WebSocket }

        #region --事件字段--
        public delegate bool InterceptorDelegate(Net net, Service.Abstract.Service service, MethodInfo method, Token token);
        #endregion

        #region --事件属性--
        /// <summary>
        /// 拦截器事件
        /// </summary>
        public event InterceptorDelegate InterceptorEvent;
        #endregion

        #region --字段--

        protected string name;
        /// <summary>
        /// Service映射表
        /// </summary>
        protected ConcurrentDictionary<string, Service.Abstract.Service> services = new ConcurrentDictionary<string, Service.Abstract.Service>();
        protected NetConfig config;
        /// <summary>
        /// Server
        /// </summary>
        protected Server.Abstract.Server server;
        /// <summary>
        /// Net类型
        /// </summary>
        protected NetType type;
        /// <summary>
        /// 插件管理器
        /// </summary>
        protected PluginManager pluginManager;

        #endregion

        #region --属性--
        public NetConfig Config { get => config; set => config = value; }
        public ConcurrentDictionary<string, Service.Abstract.Service> Services { get => services; }
        public Server.Abstract.Server Server { get => server; set => server = value; }
        public NetType Type { get => type; set => type = value; }
        protected PluginManager PluginManager { get => pluginManager; set => pluginManager = value; }
        public string Name { get => name; set => name = value; }


        #endregion

        #region --方法--
        /// <summary>
        /// 部署节点
        /// </summary>
        /// <returns></returns>
        public virtual bool Publish()
        {
            try
            {
                if (config.PluginMode)
                {
                    pluginManager.Listen();
                }
            }
            catch (TrackException e)
            {
                OnException(e);
            }
            catch (Exception e)
            {
                OnException(new TrackException(e));
            }
            return true;
        }

        public Net(string name)
        {
            this.name = name;
            pluginManager = new PluginManager(this);
        }

        public bool OnInterceptor(Service.Abstract.Service service, MethodInfo method, Token token)
        {
            if (InterceptorEvent != null)
            {
                foreach (InterceptorDelegate item in InterceptorEvent.GetInvocationList())
                {
                    if (!item.Invoke(this, service, method, token)) return false;
                }
                return true;
            }
            else return true;
        }
        #endregion
    }
}
