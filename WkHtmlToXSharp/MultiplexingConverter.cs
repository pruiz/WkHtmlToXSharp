using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sanford.Threading;

namespace WkHtmlToXSharp
{
	public class MultiplexingConverter : IHtmlToPdfConverter
	{
		// Internal 'thread delegate proxy' which handles multiplexing 
		// of calls  to qk/qt from a single thread.
		private static readonly DelegateQueue _worker = new DelegateQueue();
		private WkHtmlToPdfConverter _converter = null;

		#region Events
		public event EventHandler<EventArgs<int>> Begin
		{
			add { _converter.Begin += value; }
			remove { _converter.Begin -= value; }
		}
		public event EventHandler<EventArgs<int, string>> PhaseChanged
		{
			add { _converter.PhaseChanged += value; }
			remove { _converter.PhaseChanged -= value; }
		}
		public event EventHandler<EventArgs<int, string>> ProgressChanged
		{
			add { _converter.ProgressChanged += value; }
			remove { _converter.ProgressChanged -= value; }
		}
		public event EventHandler<EventArgs<bool>> Finished
		{
			add { _converter.Finished += value; }
			remove { _converter.Finished -= value; }
		}
		public event EventHandler<EventArgs<string>> Error 
		{
			add { _converter.Error += value; }
			remove { _converter.Error -= value; }
		}
		public event EventHandler<EventArgs<string>> Warning
		{
			add { _converter.Warning += value; }
			remove { _converter.Warning -= value; }
		}
		#endregion

		public PdfGlobalSettings GlobalSettings { get { return _converter.GlobalSettings; } }
		public PdfObjectSettings ObjectSettings { get { return _converter.ObjectSettings; } }

		public MultiplexingConverter()
		{
			_worker.Invoke((Action)(() => _converter = new WkHtmlToPdfConverter()));
		}

		public byte[] Convert()
		{
			return (byte[])_worker.Invoke((Func<byte[]>)(() => _converter.Convert()));
		}

		public byte[] Convert(string inputHtml)
		{
			return (byte[])_worker.Invoke((Func<string, byte[]>)((x) => _converter.Convert(x)));
		}

		public void Dispose()
		{
			if (_converter != null)
				_worker.Invoke((Action)(() => _converter.Dispose()));

			_converter = null;
		}
	}
}
