using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WkHtmlToXSharp
{
	internal static class LibsHelper
	{
		private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static string _OSName = null;
		private static string _ResourcePath = null;
		private static Assembly Assembly = typeof(LibsHelper).Assembly;

		private static bool RunningIn64Bits { get { return IntPtr.Size == 8; } }

		[DllImport("libc")]
		static extern int uname(IntPtr buf);

		private static string GetOsName()
		{
			if (!string.IsNullOrEmpty(_OSName))
				return _OSName;

			if (Environment.OSVersion.Platform != PlatformID.Unix
				&& Environment.OSVersion.Platform != PlatformID.MacOSX)
				throw new NotImplementedException("This platform does not support uname.");

			IntPtr buf = IntPtr.Zero;
			try
			{
				buf = Marshal.AllocHGlobal(8192);
				// This is a hacktastic way of getting sysname from uname ()
				if (uname(buf) == 0)
				{
					return Marshal.PtrToStringAnsi(buf);
				}
			}
			catch { }
			finally
			{
				if (buf != IntPtr.Zero) Marshal.FreeHGlobal(buf);
			}

			return null;
		}

		private static string GetWinSubPath()
		{
			if (RunningIn64Bits)
				throw new NotSupportedException("Sorry, WkHtmlToXSharp does not (yet) support Win64.");

			return "Win32";
		}

		private static string GetUnixSubPath()
		{
			var osName = GetOsName();

			if (osName != "Linux")
				throw new NotSupportedException("Sorry, WkHtmlToXSharp does not (yet) support your OS.");

			return osName + (RunningIn64Bits ? "64" : "32");
		}

		private static string GetResourcePath()
		{
			if (!string.IsNullOrEmpty(_ResourcePath))
				return _ResourcePath;

			var pathBase = typeof(LibsHelper).Namespace + ".Libs.";

			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
					return pathBase + GetWinSubPath();
				case PlatformID.Unix:
					return pathBase + GetUnixSubPath();
			}

			throw new NotSupportedException("Sorry, WkHtmlToSharp does not support this platform at this time.");
		}

		private static void CopyStream(Stream input, Stream output)
		{
			byte[] buffer = new byte[0x1000];
			int read;
			while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
				output.Write(buffer, 0, read);
		}

		private static bool IsFileLocked(string filePath)
		{
			FileStream stream = null;

			try
			{
				stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			//file is not locked
			return false;
		}

		private static void DeployLibrary(string resource)
		{
			var outputPath = AppDomain.CurrentDomain.BaseDirectory;
			var resourcePath = GetResourcePath();
			var fileName = resource.Substring(resourcePath.Length + 1);

			fileName = Path.Combine(outputPath, fileName);

			if (File.Exists(fileName))
			{
				if (File.GetCreationTime(fileName) < File.GetCreationTime(Assembly.Location))
					return;

				if (IsFileLocked(fileName))
				{
					_Log.WarnFormat("Unable to update {0}: file in use!", fileName);
					return;
				}
			}

			_Log.InfoFormat("Deploying {0} to {1}..", fileName, outputPath);

			using (var input = Assembly.GetManifestResourceStream(resource))
			using (var output = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				CopyStream(input, output);
			}

			if (Environment.OSVersion.Platform == PlatformID.Unix) 
			{
				// Set as executable (only applies to mono)..
				var attrs = File.GetAttributes(fileName);
				File.SetAttributes(fileName, (FileAttributes)((uint)attrs | 0x80000000));
			}
		}

		public static void DeployLibraries()
		{
			var resourcesList = Assembly.GetManifestResourceNames();
			var resourcePath = GetResourcePath();

			foreach (var res in resourcesList)
			{
				if (res.StartsWith(resourcePath))
					DeployLibrary(res);
			}
		}
	}
}
