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
using System.Linq;
using System.Text;
using System.Threading;
using Sanford.Threading;

namespace WkHtmlToXSharp
{
	public class MultiplexingConverter : IHtmlToPdfConverter
	{
		private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// Internal 'thread delegate proxy' which handles multiplexing 
		// of calls  to qk/qt from a single thread.
		private static readonly DelegateQueue _worker = new DelegateQueue();
		private static WkHtmlToPdfConverter _initiWorkAround = null;
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
			lock (_worker)
			{
				// XXX: We need to keep a converter instance alive during the whole application
				//		lifetime due to some underlying's library bug by which re-initializing
				//		the API after having deinitiaized it, causes all newlly rendered pdf
				//		file to be corrupted. So we will keep this converter alive to avoid 
				//		de-initialization until app's shutdown. (pruiz)
				// See: http://code.google.com/p/wkhtmltopdf/issues/detail?id=511
				if (_initiWorkAround == null)
				{
					_Log.InfoFormat("Initializing converter infrastructure..");
					_worker.Invoke((Action)(() => _initiWorkAround = new WkHtmlToPdfConverter()));
					_Log.InfoFormat("Initialized converter infrastructure..");

					AppDomain.CurrentDomain.ProcessExit += (o, e) =>
						_worker.Invoke((Action)(() => {
							_Log.InfoFormat("Disposing converter infraestructure..");
							_initiWorkAround.Dispose();
							_initiWorkAround = null;
							_Log.InfoFormat("Disposed converter infraestructure..");
						}));
				}
			}

			_worker.Invoke((Action)(() => _converter = new WkHtmlToPdfConverter()));
		}

		public byte[] Convert()
		{
			return (byte[])_worker.Invoke((Func<byte[]>)(() => _converter.Convert()));
		}

		public byte[] Convert(string inputHtml)
		{
			return (byte[])_worker.Invoke((Func<string, byte[]>)((x) => _converter.Convert(x)), inputHtml);
		}

		public void Dispose()
		{
			if (_converter != null)
				_worker.Invoke((Action)(() => _converter.Dispose()));

			_converter = null;
		}
	}
}
