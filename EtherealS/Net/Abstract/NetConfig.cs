using System;
using System.Collections.Generic;
using System.Reflection;
using EtherealS.Net.Interface;
using EtherealS.Server.Abstract;

namespace EtherealS.Net.Abstract
{
    /// <summary>
    /// Ethereal网关
    /// </summary>
    public class NetConfig:INetConfig
    {
        #region --委托--
        public delegate bool InterceptorDelegate(Service.Abstract.Service service,MethodInfo method,Token token);
        #endregion

        #region --事件属性--

        /// <summary>
        /// 网络级拦截器事件
        /// </summary>
        public event InterceptorDelegate InterceptorEvent;

        #endregion

        #region --字段--
        /// <summary>
        /// 分布式模式是否开启
        /// </summary>
        private bool netNodeMode = false;
        /// <summary>
        /// 分布式IP组
        /// </summary>
        private List<Tuple<string,EtherealC.Client.Abstract.ClientConfig>> netNodeIps;
        /// <summary>
        /// 网络节点心跳周期
        /// </summary>
        private int netNodeHeartbeatCycle = 10000;//默认60秒心跳一次


        #endregion

        #region --属性--

        public bool NetNodeMode { get => netNodeMode; set => netNodeMode = value; }
        public List<Tuple<string, EtherealC.Client.Abstract.ClientConfig>> NetNodeIps { get => netNodeIps; set => netNodeIps = value; }
        public int NetNodeHeartbeatCycle { get => netNodeHeartbeatCycle; set => netNodeHeartbeatCycle = value; }

        #endregion

        #region --方法--
        public bool OnInterceptor(Service.Abstract.Service service,MethodInfo method,Token token)
        {
            if (InterceptorEvent != null)
            {
                foreach (InterceptorDelegate item in InterceptorEvent.GetInvocationList())
                {
                    if (!item.Invoke(service, method, token)) return false;
                }
                return true;
            }
            else return true;
        }
        #endregion
    }
}
