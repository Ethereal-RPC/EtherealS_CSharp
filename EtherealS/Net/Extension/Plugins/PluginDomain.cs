using EtherealS.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace EtherealS.Net.Extension.Plugins
{
    public class PluginDomain
    {
        #region --字段--
        private AssemblyLoadContext assemlyLoad;
        #endregion

        #region --属性--
        /// <summary>
        /// 获取当前实例加载的程序集名称
        /// </summary>
        public string AssemblyName { get; set; }
        /// <summary>
        /// 获取当前实例加载的根目录
        /// </summary>
        public string BaseDirectory { get; set; }
        /// <summary>
        /// 获取当前实例的程序集路径
        /// </summary>
        public string AssemblyPath { get; set; }
        /// <summary>
        /// 获取当前实例的私有依赖目录
        /// </summary>
        public string PrivateLibDirectory { get; set; }
        /// <summary>
        /// 获取当前实例的数据目录
        /// </summary>
        public string DataDirectory { get; set; }
        /// <summary>
        /// 获取当前实例的缓存目录
        /// </summary>
        public string CachesDirectory { get; set; }
        /// <summary>
        /// 获取当前实例的影卷复制目录
        /// </summary>
        public string ShadowCopyDirectory { get; set; }
        /// <summary>
        /// 获取当前实例入口文件的影卷复制目录
        /// </summary>
        public string AssemblyShadowCopyPath { get; set; }
        /// <summary>
        /// 获取当前实例是否已经初始化
        /// </summary>
        public bool IsInitialized { get; set; }
        /// <summary>
        /// 获取当前实例是否已经销毁
        /// </summary>
        public bool IsDisposed { get; set; }
        public AssemblyLoadContext AssemlyLoad { get => assemlyLoad; set => assemlyLoad = value; }
        public Plugin Plugin {get;set;}
        #endregion

        /// <summary>
        /// 初始化 <see cref="PluginDomain"/> 类的新实例
        /// </summary>
        /// <param name="assemblyPath">要加载的程序集绝对路径</param>
        /// <param name="baseDirectory">程序域的根目录</param>
        /// <param name="libDirectory">程序域的私有依赖目录</param>
        /// <param name="dataDirectory">程序域的数据路径</param>
        /// <param name="cachesDirectory">程序域的缓存目录</param>
        /// <param name="tempDirectory">程序域的临时目录</param>
        /// <exception cref="ArgumentException">assemblyPath、baseDirectory、binDirectory、dataDirectory 或 tempDirectory 不能是 <see langword="null"/> 或为空</exception>

        public PluginDomain(string assemblyPath, string baseDirectory, string libDirectory, string dataDirectory, string cachesDirectory, string shadowCopyDirectory)
        {
            #region 参数检查
            if (string.IsNullOrEmpty(assemblyPath))
            {
                throw new ArgumentException($"“{nameof(assemblyPath)}”不能是 Null 或为空。", nameof(assemblyPath));
            }
            if (string.IsNullOrEmpty(baseDirectory))
            {
                throw new ArgumentException($"“{nameof(baseDirectory)}”不能是 Null 或为空。", nameof(baseDirectory));
            }
            if (string.IsNullOrEmpty(libDirectory))
            {
                throw new ArgumentException($"“{nameof(libDirectory)}”不能是 Null 或为空。", nameof(libDirectory));
            }
            if (string.IsNullOrEmpty(dataDirectory))
            {
                throw new ArgumentException($"“{nameof(dataDirectory)}”不能是 Null 或为空。", nameof(dataDirectory));
            }
            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException("未找到有效的程序集", assemblyPath);
            }
            #endregion

            #region 初始化目录
            this.AssemblyPath = assemblyPath;
            this.AssemblyName = Path.GetFileNameWithoutExtension(this.AssemblyPath);   // 获取文件名
            this.BaseDirectory = baseDirectory;
            this.DataDirectory = dataDirectory;
            this.PrivateLibDirectory = libDirectory;
            this.CachesDirectory = cachesDirectory;
            this.ShadowCopyDirectory = shadowCopyDirectory;

            if (!Directory.Exists(this.PrivateLibDirectory))
                Directory.CreateDirectory(this.PrivateLibDirectory);
            if (!Directory.Exists(this.DataDirectory))
                Directory.CreateDirectory(this.DataDirectory);
            if (!Directory.Exists(this.CachesDirectory))
                Directory.CreateDirectory(this.CachesDirectory);
            if (!Directory.Exists(this.ShadowCopyDirectory))
                Directory.CreateDirectory(this.ShadowCopyDirectory);
            #endregion
        }
        public bool Initialize(Abstract.Net net)
        {
            assemlyLoad = new AssemblyLoadContext(null, true);

            #region 加载Lib
            //将所有Lib复制到卷影目录
            foreach (FileInfo fileInfo in new DirectoryInfo(PrivateLibDirectory).GetFiles("*.dll", SearchOption.AllDirectories))
            {
                File.Copy(fileInfo.FullName, Path.Combine(ShadowCopyDirectory, fileInfo.Name), true);
            }
            //载入Lib
            foreach (FileInfo file in new DirectoryInfo(ShadowCopyDirectory).GetFiles("*.dll", SearchOption.AllDirectories))
            {
                //考虑到有用户会把EtherealS.dll误放入Lib导致重复加载
                if (file.Name != "EtherealS.dll") assemlyLoad.LoadFromAssemblyPath(file.FullName);
            }
            #endregion

            #region 加载程序
            AssemblyShadowCopyPath = Path.Combine(ShadowCopyDirectory, Path.GetFileName(AssemblyPath));
            //将核心入口Dll复制到卷影目录
            File.Copy(AssemblyPath, AssemblyShadowCopyPath, true);
            //加载Assembly
            Assembly assembly = assemlyLoad.LoadFromAssemblyPath(this.AssemblyShadowCopyPath);
            //扫描IPlugin
            foreach (var type in assembly.GetTypes())
            {
                if(type.GetCustomAttribute<PluginAttribute>() != null)
                {
                    Plugin = Activator.CreateInstance(type) as Plugin;
                    IsInitialized = true;
                    Plugin.Initialize(net);
                }
            }
            #endregion
            return true;
        }
        public bool UnInitialize(Abstract.Net net)
        {
            if (IsInitialized)
            {
                Plugin.UnInitialize(net);
                Plugin = null;
                IsInitialized = false;
            }
            if (!IsDisposed)
            {
                //创建弱引用，跟踪销毁情况。
                WeakReference weakReference = new WeakReference(assemlyLoad, true);
                assemlyLoad = null;
                (weakReference.Target as AssemblyLoadContext).Unload();
                //一般第二次,IsAlive就会变为False,即AssemblyLoadContext卸载.
                for (int i = 0; weakReference.IsAlive && (i < 10); i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                IsDisposed = true;
            }
            return true;
        }
    }
}
