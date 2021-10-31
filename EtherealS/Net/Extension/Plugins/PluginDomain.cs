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
		private PluginManager manager;
		private List<Service.Abstract.Service> services = new List<Service.Abstract.Service>();
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
        public PluginManager Manager { get => manager; set => manager = value; }
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

        public PluginDomain(string assemblyPath, string baseDirectory, string libDirectory, string dataDirectory, string cachesDirectory,string shadowCopyDirectory)
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
			#region 初始化目录配置
			assemlyLoad = new AssemblyLoadContext(AssemblyName, true);
			//载入依赖项
			foreach (FileInfo file in new DirectoryInfo(PrivateLibDirectory).GetFiles("*.dll", SearchOption.AllDirectories))
			{
				assemlyLoad.LoadFromAssemblyPath(file.DirectoryName);
			}
			//将所有Lib复制到卷影目录
			foreach (FileInfo fileInfo in new DirectoryInfo(PrivateLibDirectory).GetFiles("*.dll", SearchOption.AllDirectories))
			{
				File.Copy(fileInfo.FullName, Path.Combine(ShadowCopyDirectory, fileInfo.Name));
			}
			AssemblyShadowCopyPath = Path.Combine(ShadowCopyDirectory, Path.GetFileName(AssemblyPath));
			//将核心入口Dll复制到卷影目录
			File.Replace(AssemblyPath, AssemblyShadowCopyPath, null);
			#endregion

			#region 加载程序
			//加载Assembly
			Assembly assembly = assemlyLoad.LoadFromAssemblyPath(this.AssemblyShadowCopyPath);
			//扫描Service
			foreach (var type in assembly.GetTypes())
			{
				Service.Attribute.Service serviceAttribute = type.GetCustomAttribute<Service.Attribute.Service>();
				if (serviceAttribute != null && serviceAttribute.Plugin)
				{
					if (type.BaseType == typeof(Service.Abstract.Service))
					{
						Service.Abstract.Service service = Activator.CreateInstance(type) as Service.Abstract.Service;
						ServiceCore.Register(net, service);
						services.Add(service);
						service.PluginDomain = this;
					}
				}
			}
			#endregion
			return true;
		}
		public bool UnInitialize()
        {
			foreach(Service.Abstract.Service service in services)
            {
				services.Remove(service);
				service.PluginDomain = null;
				ServiceCore.UnRegister(service);
            }
			AssemlyLoad.Unload();
			assemlyLoad = null;
			//清理影卷复制目录
			Directory.Delete(ShadowCopyDirectory);
			return true;
		}
	}
}
