using System;
using System.Collections.Generic;
using System.Text;
using EtherealS.Core.Model;
using EtherealS.Net.NetNode.NetNodeServer.Request;
using EtherealS.Server.Abstract;
using EtherealS.Service.Attribute;
using EtherealS.Service.WebSocket;
using ServiceConfig = EtherealS.Service.Abstract.ServiceConfig;

namespace EtherealS.Net.NetNode.NetNodeServer.Service
{
    public class ServerNodeService:WebSocketService
    {
        #region --字段--
        /// <summary>
        /// 节点信息
        /// </summary>
        private Dictionary<string, Tuple<BaseToken,Model.NetNode>> netNodes = new ();
        /// <summary>
        /// 分布式请求
        /// </summary>
        private ClientNodeRequest distributeRequest;

        private Random random = new Random();
        #endregion

        #region --属性--

        public ClientNodeRequest DistributeRequest { get => distributeRequest; set => distributeRequest = value; }
        public Dictionary<string, Tuple<BaseToken, Model.NetNode>> NetNodes { get => netNodes; set => netNodes = value; }
        #endregion


        #region --RPC方法--

        /// <summary>
        /// 注册节点
        /// </summary>
        /// <param name="token">Tooken</param>
        /// <param name="netNode">节点信息</param>
        /// <returns></returns>
        [EtherealS.Service.Attribute.Service]
        public bool Register(BaseToken token, Model.NetNode netNode)
        {
            token.key = $"{netNode.Name}-{string.Join("::",netNode.Prefixes)}";
            //自建一份字典做缓存
            if(NetNodes.TryGetValue((string)token.key,out Tuple<BaseToken, Model.NetNode> value))
            {
                value.Item1.DisConnectEvent -= Sender_DisConnectEvent;
                NetNodes.Remove((string)token.key);
            }
            NetNodes.Add((string)token.key, new (token,netNode));
            token.DisConnectEvent += Sender_DisConnectEvent;
            OnLog(TrackLog.LogCode.Runtime, $"{token.key}注册节点成功");
            PrintNetNode();
            return true;
        }


        /// <summary>
        /// 获取对应服务的网络节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        [EtherealS.Service.Attribute.Service]
        public Model.NetNode GetNetNode(BaseToken sender, string serviceName)
        {
            //负载均衡的优化算法后期再写，现在采取随机分配
            List<Model.NetNode> nodes = new List<Model.NetNode>();
            foreach(Tuple<BaseToken, Model.NetNode> tuple in NetNodes.Values)
            {
                if (tuple.Item2.Services.ContainsKey(serviceName))
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
            NetNodes.Remove((string)token.key);
            OnLog(TrackLog.LogCode.Runtime,$"成功删除节点{(token.key)}");
            PrintNetNode();
        }

        private void PrintNetNode()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Tuple<BaseToken, Model.NetNode> tuple in NetNodes.Values)
            {
                sb.AppendLine($"{tuple.Item2.Name}");
            }
            OnLog(TrackLog.LogCode.Runtime, $"当前节点信息:\n{sb}");
        }
        #endregion
        
    }
}
