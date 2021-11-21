namespace EtherealS.Net.Extension.Plugins
{
    public class PluginConfig
    {
        #region --字段--
        /// <summary>
        /// 插件目录
        /// </summary>
        private string baseDirectory;
        #endregion

        #region --属性--
        public string BaseDirectory { get => baseDirectory; set => baseDirectory = value; }
        #endregion
    }
}
