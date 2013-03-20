using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sanford.Threading;

namespace WkHtmlToXSharp
{
	public class GenericMultiplexingConverter<T> : IHtmlToXConverter where T: class, IHtmlToXConverter, IDisposable, new()
	{
		private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// Internal 'thread delegate proxy' which handles multiplexing 
		// of calls  to qk/qt from a single thread.
		private static readonly DelegateQueue _worker = new DelegateQueue();
        private static T _initiWorkAround = null;
        protected T _converter = null;

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

		public IGlobalSettings GlobalSettings { get { return _converter.GlobalSettings; } }
		public IObjectSettings ObjectSettings { get { return _converter.ObjectSettings; } }

        public GenericMultiplexingConverter()
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
					_worker.Invoke((Action)(() => _initiWorkAround = new T()));
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

			_worker.Invoke((Action)(() => _converter = new T()));
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

    
    public class PdfMultiplexingConverter : GenericMultiplexingConverter<WkHtmlToPdfConverter>, IHtmlToPdfConverter
    {
        public PdfObjectSettings PdfObjectSettings { get { return _converter.PdfObjectSettings; } }
        public PdfGlobalSettings PdfGlobalSettings { get { return _converter.PdfGlobalSettings; } }
    }

    // backwards compatibility
    public class MultiplexingConverter : PdfMultiplexingConverter  {}

    public class ImageMultiplexingConverter : GenericMultiplexingConverter<WkHtmlToImageConverter>, IHtmlToImageConverter
    {
        public ImageGlobalSettings ImageGlobalSettings { get { return _converter.ImageGlobalSettings; } }
    }


}
