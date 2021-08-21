using System;
using System.Collections.Generic;
using System.Text;
using EtherealS.Model;
using EtherealS.RPCNet.Model;
using EtherealS.RPCNet.Server.Request;

namespace EtherealS.RPCNet.Server.Service
{
    public class ServerNodeService
    {
        #region --字段--
        /// <summary>
        /// 对应服务的Config信息
        /// </summary>
        [Attribute.Service.ServiceConfig]
        private RPCService.ServiceConfig config = null;
        /// <summary>
        /// 节点信息
        /// </summary>
        private Dictionary<string, NetNode> netNodes = new Dictionary<string, NetNode>();
        /// <summary>
        /// 分布式请求
        /// </summary>
        private ClientNodeRequest distributeRequest;

        private Random random = new Random();
        #endregion

        #region --属性--
        public Dictionary<string, NetNode> NetNodes { get => netNodes; set => netNodes = value; }
        public ClientNodeRequest DistributeRequest { get => distributeRequest; set => distributeRequest = value; }
        #endregion


        #region --RPC方法--

        /// <summary>
        /// 注册节点
        /// </summary>
        /// <param name="token">Tooken</param>
        /// <param name="netNode">节点信息</param>
        /// <returns></returns>
        [Attribute.RPCService]
        public bool Register(BaseUserToken token, NetNode netNode)
        {
            //传递连接体
            token.ReplaceToken(netNode);
            //自建一份字典做缓存
            netNodes.Add(netNode.Name, netNode);
            netNode.DisConnectEvent += Sender_DisConnectEvent;
            Console.WriteLine($"{netNode.Name}-{netNode.Ip}-{netNode.Port}注册节点成功");
            StringBuilder sb = new StringBuilder();
            foreach(NetNode node in netNodes.Values)
            {
                sb.AppendLine($"{node.Name}");
            }
            Console.WriteLine($"当前节点信息:\n{sb}");
            return true;
        }


        /// <summary>
        /// 获取对应服务的网络节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="servicename"></param>
        /// <returns></returns>
        [Attribute.RPCService]
        public NetNode GetNetNode(BaseUserToken sender, string servicename)
        {
            //负载均衡的优化算法后期再写，现在采取随机分配
            List<NetNode> nodes = new List<NetNode>();
            foreach(NetNode node in netNodes.Values)
            {
                if (node.Services.ContainsKey(servicename))
                {
                    nodes.Add(node);
                }
            }
            if(nodes.Count > 0)
            {
                //成功返回对应节点
                return nodes[random.Next(0, nodes.Count)];
            }
            return null;
        }

        #endregion


        #region --普通方法--


        /// <summary>
        /// 如果断开连接，字典中删掉该节点
        /// </summary>
        /// <param name="token"></param>
        private void Sender_DisConnectEvent(BaseUserToken token)
        {
            if (token is NetNode)
            {
                NetNodes.Remove((token as NetNode).Name);
                Console.WriteLine($"成功删除节点{(token as NetNode).Name}");
                StringBuilder sb = new StringBuilder();
                foreach (NetNode node in netNodes.Values)
                {
                    sb.AppendLine($"{node.Name}");
                }
                Console.WriteLine($"当前节点信息:\n{sb}");
            }
            else Console.WriteLine($"{token.Key} 转NetNode失败");
        }

        #endregion

    }
}
