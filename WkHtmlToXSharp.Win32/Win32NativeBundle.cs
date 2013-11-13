using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace WkHtmlToXSharp
{
	public class Win32NativeBundle : INativeLibraryBundle
	{
		private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
		private static readonly string ResourcesPath = typeof(Win32NativeBundle).Namespace + ".Libs.";

		#region INativeLibraryBundle Members

		public bool SupportsCurrentPlatform
		{
			get {
				return Environment.OSVersion.Platform == PlatformID.Win32NT && !WkHtmlToXLibrariesManager.RunningIn64Bits;
			}
		}

		private void DeployLibrary(WkHtmlToXLibrariesManager manager, string resource)
		{
			var fileName = resource.Substring(ResourcesPath.Length);

			using (var stream = Assembly.GetManifestResourceStream(resource))
			{
				manager.DeployLibrary(stream, fileName, File.GetLastWriteTime(Assembly.Location));
			}
		}

		public void DeployBundle(WkHtmlToXLibrariesManager manager)
		{
			if (manager == null) throw new ArgumentNullException("manager");

			var resourcesList = Assembly.GetManifestResourceNames();

			foreach (var res in resourcesList)
			{
				if (res.StartsWith(ResourcesPath))
					DeployLibrary(manager, res);
			}
		}

		#endregion
	}
}
