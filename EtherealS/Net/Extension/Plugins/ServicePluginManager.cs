using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace EtherealS.Net.Extension.Plugins
{
    public class ServicePluginManager
    {
		#region --字段--
		private bool _isAuthorized;
		private bool _isInitialized;
		private bool _isDisposed;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取当前实例的唯一标识
		/// </summary>
		public int Id { get => this.AssemblyDomain.Id; }
		/// <summary>
		/// 获取当前实例加载的程序集名称
		/// </summary>
		public string AssemblyName { get; }
		/// <summary>
		/// 获取当前实例的程序集路径
		/// </summary>
		public string AssemblyPath { get; }
		/// <summary>
		/// 获取当前实例的程序集复制路径
		/// </summary>
		public string AssemblyCopyPath { get; }
		/// <summary>
		/// 获取当前实例的运行目录
		/// </summary>
		public string RuntimeDirectory { get; }
		/// <summary>
		/// 获取当前实例的私有依赖目录
		/// </summary>
		public string PrivateBinDirectory { get; }
		/// <summary>
		/// 获取当前实例的数据目录
		/// </summary>
		public string DataDirectory { get; }
		/// <summary>
		/// 获取当前实例的临时目录
		/// </summary>
		public string TempDirectory { get; }
		/// <summary>
		/// 获取当前实例的缓存目录
		/// </summary>
		public string CachesDirectory { get; }
		/// <summary>
		/// 获取当前实例的影卷复制目录
		/// </summary>
		public string ShadowCopyDirectory { get; }

		/// <summary>
		/// 获取当前实例的程序集域
		/// </summary>
		public AppDomain AssemblyDomain { get; }

		/// <summary>
		/// 获取当前实例是否已经授权
		/// </summary>
		public bool IsAuthorized { get => _isAuthorized; }
		/// <summary>
		/// 获取当前实例是否已经初始化
		/// </summary>
		public bool IsInitialized => this._isInitialized;
		#endregion

		/// <summary>
		/// 初始化 <see cref="PluginDomain"/> 类的新实例
		/// </summary>
		/// <param name="assemblyDirectory">要加载的程序集绝对路径</param>
		/// <param name="baseDirectory">程序域的根目录</param>
		/// <param name="binDirectory">程序域的私有依赖目录</param>
		/// <param name="dataDirectory">程序域的数据路径</param>
		/// <param name="cachesDirectory">程序域的缓存目录</param>
		/// <param name="tempDirectory">程序域的临时目录</param>
		/// <exception cref="ArgumentException">assemblyPath、baseDirectory、binDirectory、dataDirectory 或 tempDirectory 不能是 <see langword="null"/> 或为空</exception>

		public ServicePluginManager(string assemblyDirectory, string baseDirectory, string binDirectory, string dataDirectory, string cachesDirectory, string tempDirectory)
		{
			#region 参数检查
			if (string.IsNullOrEmpty(assemblyDirectory))
			{
				throw new ArgumentException($"“{nameof(assemblyDirectory)}”不能是 Null 或为空。", nameof(assemblyDirectory));
			}
			if (string.IsNullOrEmpty(baseDirectory))
			{
				throw new ArgumentException($"“{nameof(baseDirectory)}”不能是 Null 或为空。", nameof(baseDirectory));
			}
			if (string.IsNullOrEmpty(binDirectory))
			{
				throw new ArgumentException($"“{nameof(binDirectory)}”不能是 Null 或为空。", nameof(binDirectory));
			}
			if (string.IsNullOrEmpty(dataDirectory))
			{
				throw new ArgumentException($"“{nameof(dataDirectory)}”不能是 Null 或为空。", nameof(dataDirectory));
			}
			if (string.IsNullOrEmpty(tempDirectory))
			{
				throw new ArgumentException($"“{nameof(tempDirectory)}”不能是 Null 或为空。", nameof(tempDirectory));
			}

			if (!File.Exists(assemblyDirectory))
			{
				throw new FileNotFoundException("未找到有效的程序集", assemblyDirectory);
			}
			#endregion

			#region 初始化目录
			this.AssemblyPath = assemblyDirectory;
			this.AssemblyName = Path.GetFileNameWithoutExtension(this.AssemblyPath);   // 获取文件名
			this.RuntimeDirectory = baseDirectory;
			this.DataDirectory = Path.Combine(dataDirectory, this.AssemblyName);
			this.TempDirectory = Path.Combine(tempDirectory, this.AssemblyName);
			this.PrivateBinDirectory = Path.Combine(this.TempDirectory, "bin");
			this.AssemblyCopyPath = Path.Combine(this.TempDirectory, $"{Path.GetFileName(this.AssemblyPath)}");
			this.CachesDirectory = cachesDirectory;
			this.ShadowCopyDirectory = Path.Combine(this.CachesDirectory, this.AssemblyName);

			if (!Directory.Exists(this.PrivateBinDirectory))
				Directory.CreateDirectory(this.PrivateBinDirectory);
			if (!Directory.Exists(this.DataDirectory))
				Directory.CreateDirectory(this.DataDirectory);
			if (!Directory.Exists(this.TempDirectory))
				Directory.CreateDirectory(this.TempDirectory);
			if (!Directory.Exists(this.ShadowCopyDirectory))
				Directory.CreateDirectory(this.ShadowCopyDirectory);
			#endregion

			#region 
			AssemblyLoadContext assemblyLoadContext = new AssemblyLoadContext(AssemblyName,true);
			File.Replace(AssemblyPath, AssemblyCopyPath, null);
			Assembly assembly = assemblyLoadContext.LoadFromAssemblyPath(this.ShadowCopyDirectory);
			
			#endregion


		}
	}
}
