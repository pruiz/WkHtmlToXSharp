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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

using SysAssembly = System.Reflection.Assembly;

namespace WkHtmlToXSharp
{
	public class WkHtmlToXLibrariesManager
	{
		private static readonly string _OutputPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
		private static readonly object _lock = new object();
		private static readonly HashSet<INativeLibraryBundle> _bundles = new HashSet<INativeLibraryBundle>();
		private static bool _done = false;

		public static bool RunningIn64Bits { get { return IntPtr.Size == 8; } }
		public static string RunningOsName { get { return NativeCalls.GetOsName(); } }

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

		private static void DeployLibraries()
		{
			var manager = new WkHtmlToXLibrariesManager();

			foreach (var bundle in _bundles.Where(x => x.SupportsCurrentPlatform))
			{
				Debug.WriteLine(string.Format("Attempting to deploy bundle: {0}", bundle));

				try
				{
					bundle.DeployBundle(manager);
				}
				catch (Exception ex)
				{
					var msg = string.Format("Deploying of bundle {0} failed.", bundle.GetType().Name);
					throw new ApplicationException(msg, ex);
				}
			}
		}

		internal static void InitializeNativeLibrary()
		{
			lock (_lock)
			{
				if (!_done)
				{
					DeployLibraries();
					_done = true;
				}
			}
		}

		public static void Register(INativeLibraryBundle bundle)
		{
			if (bundle == null) throw new ArgumentNullException("bundle");

			if (!_bundles.Contains(bundle))
			{
				Debug.WriteLine(string.Format("Registering bundle: {0}", bundle));
				_bundles.Add(bundle);
			}
		}

		#region Instance Members		
		public void DeployLibrary(Stream stream, string fileName, DateTime timestamp)
		{
			var compressed = fileName.EndsWith(".gz", StringComparison.InvariantCultureIgnoreCase);

			fileName = compressed ? Path.GetFileNameWithoutExtension(fileName) : fileName;
			fileName = Path.Combine(_OutputPath, fileName);

			if (File.Exists(fileName))
			{
				if (File.GetLastWriteTime(fileName) > timestamp)
					return;

				if (IsFileLocked(fileName))
				{
					Trace.TraceWarning("Unable to update {0}: file in use!", fileName);
					return;
				}
			}

			Trace.TraceInformation("Deploying embedded {0} to {1}..", Path.GetFileName(fileName), _OutputPath);

			byte[] hash = null;

			using (var input = compressed ? new GZipStream(stream, CompressionMode.Decompress, false) : stream)
			using (var output = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				hash = CopyStream(input, output);
			}

			Trace.TraceInformation("Deployed {0} with md5sum: {1}.", fileName, string.Concat(hash.Select(b => b.ToString("X2")).ToArray()));

			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				// Set as executable (only applies to mono)..
				var attrs = File.GetAttributes(fileName);
				File.SetAttributes(fileName, (FileAttributes)((uint)attrs | 0x80000000));
			}
		}
		#endregion
	}
}
