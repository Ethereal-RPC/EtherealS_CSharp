using EtherealS.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Net.Extension.Plugins
{
    public class PluginManager
    {
        #region --字段--
        private string netName;
        private List<PluginDomain> plugins = new List<PluginDomain>();
        private PluginConfig config = new PluginConfig();
        #endregion

        #region --属性--

        public string NetName { get => netName; set => netName = value; }
        public PluginConfig Config { get => config; set => config = value; }

        #endregion

        public PluginManager(string netName)
        {
            this.netName = netName;
        }
        public void Listen()
        {
            if (NetCore.Get(netName, out Abstract.Net net))
            {
                if (!Directory.Exists(config.BaseDirectory))
                {
                    Directory.CreateDirectory(config.BaseDirectory);
                }
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = config.BaseDirectory;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";
                watcher.IncludeSubdirectories = true;
                watcher.Created += ReLoad;
                watcher.Changed += ReLoad;
                watcher.Renamed += ReName;
                watcher.Deleted += ReLoad;
                watcher.EnableRaisingEvents = true;
                //初始扫描所有文件，寻找入口
                foreach(FileInfo fileInfo in new DirectoryInfo(config.BaseDirectory).GetFiles("*.dll",SearchOption.AllDirectories))
                {
                    //扫描入口
                    foreach (var type in Assembly.LoadFile(fileInfo.FullName).GetTypes())
                    {
                        Service.Attribute.Service serviceAttribute = type.GetCustomAttribute<Service.Attribute.Service>();
                        if (serviceAttribute != null && serviceAttribute.Plugin)
                        {
                            //找到了入口程序集
                            if (type.BaseType == typeof(Service.Abstract.Service))
                            {
                                LoadPlugin(net, fileInfo);
                            }
                            else throw new TrackException(TrackException.ErrorCode.Runtime, $"{fileInfo.FullName}-{type.BaseType}并非Service,您可能错误标注了");
                        }
                    }
                }
            }
            else throw new TrackException(TrackException.ErrorCode.Runtime, $"PluginManager未找到Net:{netName}");
        }

        private void ReName(object sender, RenamedEventArgs e)
        {
            foreach(PluginDomain plugin in plugins)
            {
                if(plugin.AssemblyPath == e.OldFullPath)
                {
                    plugin.AssemblyPath = e.FullPath;
                    plugin.AssemblyName = e.Name;
                }
            }
        }

        private void ReLoad(object sender, FileSystemEventArgs e)
        {
            if (!NetCore.Get(netName, out Abstract.Net net))
            {
                throw new TrackException(TrackException.ErrorCode.Runtime, $"PluginManager未找到Net:{netName}");
            }
            else if (Path.GetDirectoryName(e.FullPath).Split(@"\").Last().Equals("shadow"))
            {
                //无视影卷目录复制操作
            }
            DirectoryInfo directory = new DirectoryInfo(e.FullPath);
            //到插件根目录停止扫描
            while(directory.FullName != config.BaseDirectory)
            {
                foreach (FileInfo fileInfo in directory.GetFiles("*.dll", SearchOption.TopDirectoryOnly))
                {
                    //扫描入口
                    foreach (var type in Assembly.LoadFile(fileInfo.FullName).GetTypes())
                    {
                        Service.Attribute.Service serviceAttribute = type.GetCustomAttribute<Service.Attribute.Service>();
                        if (serviceAttribute != null && serviceAttribute.Plugin)
                        {
                            //找到了入口程序集
                            if (type.BaseType == typeof(Service.Abstract.Service))
                            {
                                LoadPlugin(net, fileInfo);
                            }
                            else throw new TrackException(TrackException.ErrorCode.Runtime, $"{fileInfo.FullName}-{type.BaseType}并非Service,您可能错误标注了");
                        }
                    }
                }
                //继续扫描上一级
                directory = directory.Parent;
            }
        }
        private bool LoadPlugin(Abstract.Net net,FileInfo fileInfo)
        {
            //判断是否已载入该插件
            PluginDomain plugin = plugins.Where(plugin => plugin.AssemblyPath == fileInfo.FullName).First();
            if (plugin != null)
            {
                UnPlugin(plugin);
            }
            plugin = new PluginDomain(fileInfo.FullName, fileInfo.DirectoryName,
                fileInfo.DirectoryName + @"\lib", fileInfo.DirectoryName + @"\config", fileInfo.DirectoryName + @"\cache",
                fileInfo.DirectoryName + @"\shadow");
            plugins.Add(plugin);
            plugin.Initialize(net);
            return true;
        }
        private bool UnPlugin(PluginDomain plugin)
        {
            plugins.Remove(plugin);
            return plugin.UnInitialize();
        }
    }
}
