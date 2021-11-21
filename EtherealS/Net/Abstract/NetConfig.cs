using EtherealS.Net.Interface;

namespace EtherealS.Net.Abstract
{
    /// <summary>
    /// Ethereal网关
    /// </summary>
    public class NetConfig : INetConfig
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
        /// 网络节点心跳周期
        /// </summary>
        private int netNodeHeartbeatCycle = 20000;//默认60秒心跳一次
        /// <summary>
        /// 插件模式
        /// </summary>
        private bool pluginMode = true;

        #endregion

        #region --属性--

        public bool NetNodeMode { get => netNodeMode; set => netNodeMode = value; }
        public int NetNodeHeartbeatCycle { get => netNodeHeartbeatCycle; set => netNodeHeartbeatCycle = value; }
        public bool PluginMode { get => pluginMode; set => pluginMode = value; }

        #endregion

        #region --方法--

        #endregion
    }
}
