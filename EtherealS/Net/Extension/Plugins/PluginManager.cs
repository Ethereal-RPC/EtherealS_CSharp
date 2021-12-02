using EtherealS.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EtherealS.Net.Extension.Plugins
{
    public class PluginManager
    {
        #region --字段--
        private List<PluginDomain> plugins = new List<PluginDomain>();
        private PluginManagerConfig config = new PluginManagerConfig();
        FileSystemWatcher watcher;
        #endregion

        #region --属性--

        public Abstract.Net Net { get; set; }
        public PluginManagerConfig Config { get => config; set => config = value; }

        #endregion

        public PluginManager(Abstract.Net net)
        {
            Net =  net;
            config.BaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Nets\{net.Name}");
        }
        public void Listen()
        {
            if (!Directory.Exists(config.BaseDirectory))
            {
                Directory.CreateDirectory(config.BaseDirectory);
            }
            watcher = new FileSystemWatcher();
            watcher.Path = config.BaseDirectory;
            watcher.Filter = "*.ethereal";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                            | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = true;
            watcher.Created += Created;
            watcher.Renamed += ReNamed;
            watcher.Deleted += Deleted;
            watcher.EnableRaisingEvents = true;
            //初始扫描所有文件，寻找入口
            foreach (FileInfo fileInfo in new DirectoryInfo(config.BaseDirectory).GetFiles("*.ethereal", SearchOption.AllDirectories))
            {
                if (!fileInfo.Directory.Name.Equals("shadow")) LoadPlugin(fileInfo);
            }
        }

        private void Created(object sender, FileSystemEventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo(e.FullPath).Parent;
            if (directory.Name.Equals("shadow")) return;
            LoadPlugin(new FileInfo(e.FullPath));
        }

        private void Deleted(object sender, FileSystemEventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo(e.FullPath).Parent;
            if (directory.Name.Equals("shadow")) return;
            //判断是否已载入该插件
            PluginDomain plugin = plugins.Where(plugin => plugin.AssemblyPath == e.FullPath).FirstOrDefault();
            if (plugin != null)
            {
                UnPlugin(plugin);
            }
        }

        private void ReNamed(object sender, RenamedEventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo(e.FullPath).Parent;
            if (directory.Name.Equals("shadow")) return;
            //判断是否已载入该插件
            PluginDomain plugin = plugins.Where(plugin => plugin.AssemblyPath == e.OldFullPath).FirstOrDefault();
            if (plugin != null)
            {
                UnPlugin(plugin);
            }
            LoadPlugin(new FileInfo(e.FullPath));
        }
        private bool LoadPlugin(FileInfo fileInfo)
        {
            //判断是否已载入该插件
            PluginDomain plugin = new PluginDomain(fileInfo.FullName, fileInfo.DirectoryName,
            fileInfo.DirectoryName + @"\lib", fileInfo.DirectoryName + @"\config", fileInfo.DirectoryName + @"\cache",
            fileInfo.DirectoryName + @"\shadow");
            plugins.Add(plugin);
            plugin.Initialize(Net);
            return true;
        }
        private bool UnPlugin(PluginDomain plugin)
        {
            plugins.Remove(plugin);
            return plugin.UnInitialize(Net);
        }
    }
}
