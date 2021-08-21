using System.Collections.Generic;
using EtherealS.Model;

namespace EtherealS.RPCNet.Model
{
    public class NetNode:BaseUserToken
    {
        #region --字段--

        /// <summary>
        /// Net节点名
        /// </summary>
        private string name;
        /// <summary>
        /// 连接数量
        /// </summary>
        private long connects;
        /// <summary>
        /// 服务信息
        /// </summary>
        private Dictionary<string, ServiceNode> services;
        /// <summary>
        /// 接口信息
        /// </summary>
        private Dictionary<string, RequestNode> requests;
        /// <summary>
        /// ip地址
        /// </summary>
        private string ip;
        /// <summary>
        /// port地址
        /// </summary>
        private string port;
        /// <summary>
        /// 硬件信息
        /// </summary>
        private HardwareInformation hardwareInformation;
        #endregion

        #region --属性--

        public string Name { get => name; set => name = value; }
        public long Connects { get => connects; set => connects = value; }
        public Dictionary<string, ServiceNode> Services { get => services; set => services = value; }
        public Dictionary<string, RequestNode> Requests { get => requests; set => requests = value; }
        public string Ip { get => ip; set => ip = value; }
        public override object Key { get => name; set => name = (string)value; }
        public HardwareInformation HardwareInformation { get => hardwareInformation; set => hardwareInformation = value; }
        public string Port { get => port; set => port = value; }


        #endregion

    }
}
