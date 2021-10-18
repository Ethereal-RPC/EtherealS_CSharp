namespace EtherealS.Net.Extension.NetNode.Model
{
    public class HardwareInformation
    {
        #region --字段--

        /// <summary>
        /// 系统名称
        /// </summary>
        private string oSDescription;
        /// <summary>
        /// 系统架构
        /// </summary>
        private string oSArchitecture;
        /// <summary>
        /// 进程架构
        /// </summary>
        private string processArchitecture;
        /// <summary>
        /// 是否64位操作系统
        /// </summary>
        private string is64BitOperatingSystem;
        /// <summary>
        /// 网络接口信息
        /// </summary>
        private string networkInterfaces;
        
        #endregion

        #region --属性--

        public string OSDescription { get => oSDescription; set => oSDescription = value; }
        public string OSArchitecture { get => oSArchitecture; set => oSArchitecture = value; }
        public string ProcessArchitecture { get => processArchitecture; set => processArchitecture = value; }
        public string Is64BitOperatingSystem { get => is64BitOperatingSystem; set => is64BitOperatingSystem = value; }
        public string NetworkInterfaces { get => networkInterfaces; set => networkInterfaces = value; }

        #endregion

    }
}
