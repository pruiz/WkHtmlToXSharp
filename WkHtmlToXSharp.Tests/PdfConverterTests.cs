using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;

using WkHtmlToXSharp.Tests.Properties;

namespace WkHtmlToXSharp.Tests
{
	[TestFixture]
	public class PdfConverterTest
	{
		private sealed class LocalAuthFailureServer : IDisposable
		{
			#region Fields & Properties

			private readonly HttpListener _listener;
			private readonly Thread _thread;
			private bool _disposed;

			public string Url { get; private set; }

			#endregion

			#region .ctors

			public LocalAuthFailureServer()
			{
				var port = GetFreeTcpPort();
				Url = string.Format("http://127.0.0.1:{0}/basic-auth/user/passwd", port);

				_listener = new HttpListener();
				_listener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", port));
				_listener.Start();

				_thread = new Thread(ListenLoop)
				{
					IsBackground = true,
					Name = "LocalAuthFailureServer"
				};
				_thread.Start();
			}

			#endregion

			#region Private methods

			private static int GetFreeTcpPort()
			{
				var tcp = new TcpListener(IPAddress.Loopback, 0);
				try
				{
					tcp.Start();
					return ((IPEndPoint)tcp.LocalEndpoint).Port;
				}
				finally
				{
					tcp.Stop();
				}
			}

			private void ListenLoop()
			{
				while (_listener.IsListening)
				{
					HttpListenerContext context;
					try
					{
						context = _listener.GetContext();

						var response = context.Response;
						response.StatusCode = (int)HttpStatusCode.Unauthorized;
						response.AddHeader("WWW-Authenticate", "Basic realm=\"Fake Realm\"");
						response.Close();
					}
					catch
					{
						// Ignore response errors in test server.
					}
				}
			}

			#endregion

			public void Dispose()
			{
				if (_disposed)
					return;

				try { _listener.Stop(); } catch { }
				try { _listener.Close(); } catch { }

				if (_thread?.IsAlive == true)
					_thread.Join(TimeSpan.FromSeconds(1));

				_disposed = true;
			}
		}

		private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static string SimplePageFile = null;
		public static int count = 0;

		private void TryRegisterLibraryBundles()
		{
			var ignore = Environment.GetEnvironmentVariable("WKHTMLTOXSHARP_NOBUNDLES");

			if (ignore == null || ignore.ToLower() != "true")
			{
				// Register all available bundles..
				WkHtmlToXLibrariesManager.Register(new Linux32NativeBundle());
				WkHtmlToXLibrariesManager.Register(new Linux64NativeBundle());
				WkHtmlToXLibrariesManager.Register(new Win32NativeBundle());
				WkHtmlToXLibrariesManager.Register(new Win64NativeBundle());
			}
		}

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			TryRegisterLibraryBundles();

			// Avoid using Temp folder as WebKit doesnt like it.
			SimplePageFile = Path.GetTempFileName();
			File.Delete(SimplePageFile);
			SimplePageFile = Path.Combine(
				Path.GetDirectoryName(SimplePageFile),
				Path.GetFileNameWithoutExtension(SimplePageFile) + ".xhtml");

			File.WriteAllBytes(SimplePageFile, Resources.SimplePage_xhtml);
		}

		//[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			if (File.Exists(SimplePageFile))
				File.Delete(SimplePageFile);
		}

		private MultiplexingConverter _GetConverter()
		{
			var obj = new MultiplexingConverter();
			obj.Begin += (s, e) => _Log.DebugFormat("Conversion begin, phase count: {0}", e.Value);
			//obj.Error += (s, e) => _Log.Error(e.Value);
			obj.Warning += (s, e) => _Log.Warn(e.Value);
			//obj.PhaseChanged += (s, e) => _Log.InfoFormat("PhaseChanged: {0} - {1}", e.Value, e.Value2);
			//obj.ProgressChanged += (s, e) => _Log.InfoFormat("ProgressChanged: {0} - {1}", e.Value, e.Value2);
			obj.Finished += (s, e) => _Log.InfoFormat("Finished: {0}", e.Value ? "success" : "failed!");
			return obj;
		}

		private void _SimpleConversion()
		{
			using (var wk = _GetConverter())
			{
				_Log.DebugFormat("Performing conversion..");

				wk.GlobalSettings.Margin.Top = "0cm";
				wk.GlobalSettings.Margin.Bottom = "0cm";
				wk.GlobalSettings.Margin.Left = "0cm";
				wk.GlobalSettings.Margin.Right = "0cm";
				//wk.GlobalSettings.Out = @"c:\temp\tmp.pdf";

				wk.ObjectSettings.Web.EnablePlugins = false;
				wk.ObjectSettings.Web.EnableJavascript = false;
				wk.ObjectSettings.Page = SimplePageFile;
				//wk.ObjectSettings.Page = "http://doc.trolltech.com/4.6/qstring.html";
				wk.ObjectSettings.Load.Proxy = "none";

				var tmp = wk.Convert();

				Assert.IsNotEmpty(tmp);
				var number = 0;
				lock (this) number = count++;

				var tempfname = Path.GetTempFileName();
				var tempPath = Path.Combine(
					Path.GetDirectoryName(tempfname),
					Path.GetFileNameWithoutExtension(tempfname) + "." + (number) + ".pdf"
				);
				File.WriteAllBytes(tempPath, tmp);
			}
		}

		[Test]
		public void CanConvertFromFile()
		{
			_SimpleConversion();
		}

		#region Testing Threads handling
		class ThreadData
		{
			public Thread Thread;
			public Exception Exception;
			public ManualResetEvent WaitHandle;
		}

		void ThreadStart(object arg)
		{
			_Log.DebugFormat("New thread {0}", arg);

			var tmp = arg as ThreadData;
			try
			{
				_SimpleConversion();
			}
			catch (Exception ex)
			{
				tmp.Exception = ex;
			}
			finally
			{
				tmp.WaitHandle.Set();
			}
		}
		#endregion

		private const int ConcurrentTimeout = 95000;

		[Test]
		[Timeout(ConcurrentTimeout)]
		[RequiresThread(ApartmentState.MTA)]
		public void CanConvertConcurrently()
		{
			var error = false;
			var threads = new List<ThreadData>();

			for (int i = 0; i < 8; i++)
			{
				var tmp = new ThreadData()
				{
					Thread = new Thread(ThreadStart),
					WaitHandle = new ManualResetEvent(false)
				};
				threads.Add(tmp);
				tmp.Thread.Start(tmp);
			}

			var handles = threads.Select(x => x.WaitHandle).ToArray();
			Assert.IsTrue(WaitHandle.WaitAll(handles, ConcurrentTimeout), "At least one thread timeout");
			//WaitAll(handles);

			threads.ForEach(x => x.Thread.Abort());

			var exceptions = threads.Select(x => x.Exception).Where(x => x != null);

			foreach (var tmp in threads)
			{
				if (tmp.Exception != null)
				{
					error = true;
					var tid = tmp.Thread.ManagedThreadId;
					_Log.Error("Thread-" + tid + " failed!", tmp.Exception);
				}
			}

			Assert.IsFalse(error, "At least one thread failed!");
		}

		[Test]
		public void ConvertFromString()
		{
			using (var wk = _GetConverter())
			{
				_Log.DebugFormat("Performing conversion..");

				using (var stream = new MemoryStream(Resources.SimplePage_xhtml))
				using (var sr = new StreamReader(stream))
				{
					var str = sr.ReadToEnd();
					var tmp = wk.Convert(str);
					Assert.IsNotEmpty(tmp);
				}
			}
		}

		[Test]
		//		[Ignore("This test requires (still to be released) wkhtmltopdf v0.12+")]
		public void CanHandleAuthFailure()
		{
			using (var server = new LocalAuthFailureServer())
			using (var wk = new MultiplexingConverter())
			{
				var failed = false;

				wk.GlobalSettings.Margin.Top = "0cm";
				wk.GlobalSettings.Margin.Bottom = "0cm";
				wk.GlobalSettings.Margin.Left = "0cm";
				wk.GlobalSettings.Margin.Right = "0cm";

				wk.ObjectSettings.Load.Proxy = "none";
				wk.ObjectSettings.Load.LoadErrorHandling = LoadErrorHandlingType.abort;
				wk.ObjectSettings.Load.StopSlowScripts = true;

				wk.ObjectSettings.Web.EnablePlugins = false;
				wk.ObjectSettings.Web.EnableJavascript = false;

				wk.ObjectSettings.Page = server.Url;

				wk.Begin += (s, e) => { Console.WriteLine("==>> Begin: {0}", e.Value); };
				wk.PhaseChanged += (s, e) => { Console.WriteLine("==>> New Phase: {0} ({1})", e.Value, e.Value2); };
				wk.ProgressChanged += (s, e) => { Console.WriteLine("==>> Progress: {0} ({1})", e.Value, e.Value2); };
				wk.Error += (s, e) =>
				{
					failed = true;
					Console.WriteLine("==>> ERROR: {0}", e.Value);
				};
				wk.Finished += (s, e) => { Console.WriteLine("==>> WARN: {0}", e.Value); };

				var tmp = wk.Convert();

				Assert.IsNotNull(tmp);
				Assert.IsTrue(failed);
			}
		}
	}
}
