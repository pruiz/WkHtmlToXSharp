using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sanford.Threading;

namespace WkHtmlToXSharp
{
	public class MultiplexingConverter : IDisposable
	{
		// Internal 'thread delegate proxy' which handles multiplexing 
		// of calls  to qk/qt from a single thread.
		private static readonly DelegateQueue _worker = new DelegateQueue();
		private WkHtmlToPdfConverter _converter = null;

		public PdfGlobalSettings GlobalSettings { get { return _converter.GlobalSettings; } }
		public PdfObjectSettings ObjectSettings { get { return _converter.ObjectSettings; } }

		public MultiplexingConverter()
		{
			//lock (_worker)
				_worker.Invoke((Action)(() => _converter = new WkHtmlToPdfConverter()));
		}

		public byte[] Convert()
		{
			//lock (_worker)
				return (byte[])_worker.Invoke((Func<byte[]>)(() => _converter.Convert()));
		}

		public void Dispose()
		{
			_worker.Invoke((Action)(() => _converter.Dispose()));
		}
	}
}
