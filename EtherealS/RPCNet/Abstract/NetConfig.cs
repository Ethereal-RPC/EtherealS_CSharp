using System;
using System.Collections.Generic;
using System.Reflection;
using EtherealS.NativeServer;
using EtherealS.NativeServer.Abstract;
using EtherealS.RPCNet.Interface;
using EtherealS.RPCService.Abstract;

namespace EtherealS.RPCNet.Abstract
{
    /// <summary>
    /// Ethereal网关
    /// </summary>
    public class NetConfig:INetConfig
    {
        #region --委托--
        public delegate bool InterceptorDelegate(Service service,MethodInfo method,Token token);
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
        private List<Tuple<string,EtherealC.NativeClient.Abstract.ClientConfig>> netNodeIps;
        /// <summary>
        /// 网络节点心跳周期
        /// </summary>
        private int netNodeHeartbeatCycle = 5000;//默认60秒心跳一次


        #endregion

        #region --属性--

        public bool NetNodeMode { get => netNodeMode; set => netNodeMode = value; }
        public List<Tuple<string, EtherealC.NativeClient.Abstract.ClientConfig>> NetNodeIps { get => netNodeIps; set => netNodeIps = value; }
        public int NetNodeHeartbeatCycle { get => netNodeHeartbeatCycle; set => netNodeHeartbeatCycle = value; }

        #endregion

        #region --方法--
        public bool OnInterceptor(Service service,MethodInfo method,Token token)
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
