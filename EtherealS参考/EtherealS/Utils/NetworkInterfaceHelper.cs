using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Utils
{
    public class NetworkInterfaceHelper
    {
        public static string GetAllNetworkInterface()
        {
            StringBuilder content = new StringBuilder();
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();//获取本地计算机上网络接口的对象
            content.AppendLine("适配器个数：" + adapters.Length);
            content.AppendLine();
            foreach (NetworkInterface adapter in adapters)
            {
                content.AppendLine("描述：" + adapter.Description);
                content.AppendLine("标识符：" + adapter.Id);
                content.AppendLine("名称：" + adapter.Name);
                content.AppendLine("类型：" + adapter.NetworkInterfaceType);
                content.AppendLine("速度：" + adapter.Speed * 0.001 * 0.001 + "M");
                content.AppendLine("操作状态：" + adapter.OperationalStatus);

                // 格式化显示MAC地址                
                PhysicalAddress pa = adapter.GetPhysicalAddress();//获取适配器的媒体访问（MAC）地址
                byte[] bytes = pa.GetAddressBytes();//返回当前实例的地址
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("X2"));//以十六进制格式化
                    if (i != bytes.Length - 1)
                    {
                        sb.Append("-");
                    }
                }
                content.AppendLine("MAC 地址：" + sb);
                content.AppendLine();
            }
            return content.ToString();
        }
    }
}
