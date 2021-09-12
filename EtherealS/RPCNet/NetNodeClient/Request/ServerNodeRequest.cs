using EtherealS.RPCNet.NetNodeModel;

namespace EtherealS.RPCNet.NetNodeClient.Request
{
    public interface ServerNodeRequest
    {
        /// <summary>
        /// 注册节点信息
        /// </summary>
        /// <param name="node">节点信息</param>
        [EtherealC.Attribute.RPCRequest]
        public bool Register(NetNode node);
    }
}
