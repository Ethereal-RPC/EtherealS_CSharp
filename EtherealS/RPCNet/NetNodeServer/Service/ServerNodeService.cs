using EtherealS.NativeServer;
using EtherealS.RPCNet.NetNodeModel;
using EtherealS.RPCNet.Server.NetNodeRequest;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtherealS.RPCNet.Server.NetNodeService
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
        private Dictionary<string, Tuple<BaseToken,NetNode>> netNodes = new ();
        /// <summary>
        /// 分布式请求
        /// </summary>
        private ClientNodeRequest distributeRequest;

        private Random random = new Random();
        #endregion

        #region --属性--

        public ClientNodeRequest DistributeRequest { get => distributeRequest; set => distributeRequest = value; }
        public Dictionary<string, Tuple<BaseToken, NetNode>> NetNodes { get => netNodes; set => netNodes = value; }
        #endregion


        #region --RPC方法--

        /// <summary>
        /// 注册节点
        /// </summary>
        /// <param name="token">Tooken</param>
        /// <param name="netNode">节点信息</param>
        /// <returns></returns>
        [Attribute.RPCService]
        public bool Register(BaseToken token, NetNode netNode)
        {
            token.Key = $"{netNode.Name}-{string.Join("::",netNode.Prefixes)}";
            //自建一份字典做缓存
            if(NetNodes.TryGetValue((string)token.Key,out Tuple<BaseToken, NetNode> value))
            {
                token.DisConnectEvent -= Sender_DisConnectEvent;
                NetNodes.Remove((string)token.Key);
            }
            NetNodes.Add((string)token.Key, new (token,netNode));
            token.DisConnectEvent += Sender_DisConnectEvent;
            Console.WriteLine($"{token.Key}注册节点成功");
            StringBuilder sb = new StringBuilder();
            foreach(Tuple<BaseToken,NetNode> tuple in NetNodes.Values)
            {
                sb.AppendLine($"{tuple.Item1.Key}");
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
        public NetNode GetNetNode(BaseToken sender, string servicename)
        {
            //负载均衡的优化算法后期再写，现在采取随机分配
            List<NetNode> nodes = new List<NetNode>();
            foreach(Tuple<BaseToken, NetNode> tuple in NetNodes.Values)
            {
                if (tuple.Item2.Services.ContainsKey(servicename))
                {
                    nodes.Add(tuple.Item2);
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
        private void Sender_DisConnectEvent(BaseToken token)
        {
            NetNodes.Remove((string)token.Key);
            Console.WriteLine($"成功删除节点{(token.Key)}");
            StringBuilder sb = new StringBuilder();
            foreach (Tuple<BaseToken, NetNode> tuple in NetNodes.Values)
            {
                sb.AppendLine($"{tuple.Item2.Name}");
            }
            Console.WriteLine($"当前节点信息:\n{sb}");
        }

        #endregion

    }
}
