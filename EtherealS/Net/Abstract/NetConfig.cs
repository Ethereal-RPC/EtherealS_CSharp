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

        #endregion

        #region --事件属性--



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
        private int netNodeHeartbeatCycle = 20000;//默认60秒心跳一次


        #endregion

        #region --属性--

        public bool NetNodeMode { get => netNodeMode; set => netNodeMode = value; }
        public List<Tuple<string, EtherealC.Client.Abstract.ClientConfig>> NetNodeIps { get => netNodeIps; set => netNodeIps = value; }
        public int NetNodeHeartbeatCycle { get => netNodeHeartbeatCycle; set => netNodeHeartbeatCycle = value; }

        #endregion

        #region --方法--

        #endregion
    }
}
