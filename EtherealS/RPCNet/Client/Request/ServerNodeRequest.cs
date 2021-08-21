using EtherealS.RPCNet.Model;

namespace EtherealS.RPCNet.Client.Request
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
