#region Copyright
//
// Author: Pablo Ruiz García (pablo.ruiz@gmail.com)
//
// (C) Pablo Ruiz García 2011
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#endregion
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

using SysAssembly = System.Reflection.Assembly;

namespace WkHtmlToXSharp
{
	internal static class LibsHelper
	{
		private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly Assembly Assembly = SysAssembly.GetExecutingAssembly();
		private static readonly string _OutputPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

		private static string _OSName = null;
		private static string _ResourcePath = null;

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
			return RunningIn64Bits ? "Win64" : "Win32";
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

		private static byte[] CopyStream(Stream input, Stream output)
		{
			var hasher = HashAlgorithm.Create("MD5");
			var buffer = new byte[0x1000];
			int read;

			hasher.Initialize();
			while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				hasher.TransformBlock(buffer, 0, read, buffer, 0);
				output.Write(buffer, 0, read);
			}
			hasher.TransformFinalBlock(buffer, 0, 0);

			return hasher.Hash;
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
			var resourcePath = GetResourcePath();
			var fileName = resource.Substring(resourcePath.Length + 1);
			var compressed = fileName.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase);

			fileName = compressed ? Path.GetFileNameWithoutExtension(fileName) : fileName;
			fileName = Path.Combine(_OutputPath, fileName);

			if (File.Exists(fileName))
			{
				if (File.GetLastWriteTime(fileName) > File.GetLastWriteTime(Assembly.Location))
					return;

				if (IsFileLocked(fileName))
				{
					_Log.WarnFormat("Unable to update {0}: file in use!", fileName);
					return;
				}
			}

			_Log.InfoFormat("Deploying embedded {0} to {1}..", Path.GetFileName(fileName), _OutputPath);

			byte[] hash = null;
			var res = Assembly.GetManifestResourceStream(resource);

			using (var input = compressed ? new GZipStream(res, CompressionMode.Decompress, false) : res)
			using (var output = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				hash = CopyStream(input, output);
			}

			_Log.InfoFormat("Deployed {0} with md5sum: {1}.", fileName, string.Concat(hash.Select(b => b.ToString("X2")).ToArray()));

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
