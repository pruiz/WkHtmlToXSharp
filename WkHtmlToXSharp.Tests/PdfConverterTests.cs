using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using NUnit.Framework;

using WkHtmlToXSharp.Tests.Properties;

namespace WkHtmlToXSharp.Tests
{
	[TestFixture]
	public class PdfConverterTest
	{
		private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static string SimplePageFile = null;
		public static int count = 0;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Avoid using Temp folder as WebKit doesnt like it.
			SimplePageFile = Path.GetTempFileName();
			File.Delete(SimplePageFile);
			SimplePageFile = Path.Combine(
				Path.GetDirectoryName(SimplePageFile),
				Path.GetFileNameWithoutExtension(SimplePageFile)+ ".xhtml");

			File.WriteAllBytes(SimplePageFile, Resources.SimplePage_xhtml);
		}

		//[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			if (File.Exists(SimplePageFile))
				File.Delete(SimplePageFile);
		}

<<<<<<< HEAD
		private MultiplexingPdfConverter _GetConverter()
		{
			var obj = new MultiplexingPdfConverter();
=======
        private T _GetConverter<T,TH>() 
            where T : GenericMultiplexingConverter<TH>, IDisposable, new()
            where TH : class, IHtmlToXConverter, new()
		{
			var obj = new T();
>>>>>>> dca205949928617a22e3e0258c28f3736ce96915
			obj.Begin += (s,e) => _Log.DebugFormat("Conversion begin, phase count: {0}", e.Value);
			obj.Error += (s, e) => _Log.Error(e.Value);
			obj.Warning += (s, e) => _Log.Warn(e.Value);
			obj.PhaseChanged += (s, e) => _Log.InfoFormat("PhaseChanged: {0} - {1}", e.Value, e.Value2);
			obj.ProgressChanged += (s, e) => _Log.InfoFormat("ProgressChanged: {0} - {1}", e.Value, e.Value2);
			obj.Finished += (s, e) => _Log.InfoFormat("Finished: {0}", e.Value ? "success" : "failed!");
			return obj;
		}

		private void _SimpleConversion()
		{
			using (var wk = _GetConverter<PdfMultiplexingConverter, WkHtmlToPdfConverter>())
			{
				_Log.DebugFormat("Performing conversion..");

                wk.PdfGlobalSettings.Margin.Top = "0cm";
                wk.PdfGlobalSettings.Margin.Bottom = "0cm";
                wk.PdfGlobalSettings.Margin.Left = "0cm";
                wk.PdfGlobalSettings.Margin.Right = "0cm";
                //wk.PdfGlobalSettings.Out = @"c:\temp\tmp.pdf";

				wk.PdfObjectSettings.Web.EnablePlugins = false;
                wk.PdfObjectSettings.Web.EnableJavascript = false;
                wk.PdfObjectSettings.Page = SimplePageFile;
                //wk.PdfObjectSettings.Page = "http://doc.trolltech.com/4.6/qstring.html";
                wk.PdfObjectSettings.Load.Proxy = "none";

				var tmp = wk.Convert();

				Assert.IsNotEmpty(tmp);
				var number = 0;
				lock (this) number = count++;
				File.WriteAllBytes(@"c:\temp\tmp" + (number) + ".pdf", tmp);
			}
		}

		[Test]
		public void CanConvertFromFile()
		{
			_SimpleConversion();
		}

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

		private const int ConcurrentTimeout = 50000;

		[Test]
		[Timeout(ConcurrentTimeout)]
		[RequiresThread(ApartmentState.MTA)]
		public void CanConvertConcurrently()
		{
			var error = false;
			var threads = new List<ThreadData>();
			
			for (int i = 0; i < 5; i++)
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
            using (var wk = _GetConverter<PdfMultiplexingConverter, WkHtmlToPdfConverter>())
			{
				_Log.DebugFormat("Performing conversion..");

				using (var stream = new MemoryStream(Resources.SimplePage_xhtml))
				using (var sr = new StreamReader(stream))
				{
					var str = sr.ReadToEnd();
					var tmp = wk.Convert(str);
                    //File.WriteAllBytes("C:\\test.pdf", tmp);
					Assert.IsNotEmpty(tmp);
				}
			}
		}

        [Test]
        public void ConvertImageFromString()
        {
            using (var wk = _GetConverter<ImageMultiplexingConverter, WkHtmlToImageConverter>())
            {
                wk.ImageGlobalSettings.Fmt = "jpg";
                _Log.DebugFormat("Performing conversion..");
                using (var stream = new MemoryStream(Resources.SimplePage_xhtml))
                using (var sr = new StreamReader(stream))
                {
                    var str = sr.ReadToEnd();
                    var tmp = wk.Convert(str);
                    //File.WriteAllBytes(string.Format("C:\\test.{0}",wk.ImageGlobalSettings.Fmt), tmp);
                    Assert.IsNotEmpty(tmp);
                }
            }
        }


	}
}
