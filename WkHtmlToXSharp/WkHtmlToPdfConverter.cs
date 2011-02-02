#region Copyright
//
// Author: Pablo Ruiz García (pruiz@crt0.net)
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

using SysConvert = System.Convert;

namespace WkHtmlToXSharp
{
	/// <summary>
	/// Plain wrapper around wkhtmltox API library.
	/// </summary>
	/// <remarks>
	/// WARNING: Due to underlaying's API restrictions all call to
	/// Convert() should be made from within the same thread!!
	/// </remarks>
	public sealed class WkHtmlToPdfConverter : IHtmlToPdfConverter
	{
		#region private fields
		private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		//private static readonly object _lock = new object();

		private const string DLL_NAME = "wkhtmltox0.dll";
		private PdfGlobalSettings _globalSettings = new PdfGlobalSettings();
		private PdfObjectSettings _objectSettings = new PdfObjectSettings();
		private StringBuilder _errorString = null;
		private int _currentPhase = 0;
		private bool _disposed = false;
		#endregion

		#region Properties
		public PdfGlobalSettings GlobalSettings { get { return _globalSettings; } }
		public PdfObjectSettings ObjectSettings { get { return _objectSettings; } }
		#endregion

		#region Events
		public event EventHandler<EventArgs<int>> Begin = delegate { };
		public event EventHandler<EventArgs<int, string>> PhaseChanged = delegate { };
		public event EventHandler<EventArgs<int, string>> ProgressChanged = delegate { };
		public event EventHandler<EventArgs<bool>> Finished = delegate { };
		public event EventHandler<EventArgs<string>> Error = delegate { };
		public event EventHandler<EventArgs<string>> Warning = delegate { };
		#endregion

		#region P/Invokes
		[DllImport(DLL_NAME)]
		static extern string wkhtmltopdf_version();

		[DllImport(DLL_NAME)]
		static extern bool wkhtmltopdf_init(int use_graphics);

		[DllImport(DLL_NAME)]
		static extern bool wkhtmltopdf_deinit();

		[DllImport(DLL_NAME)]
		static extern bool wkhtmltopdf_extended_qt();

		[DllImport(DLL_NAME)]
		static extern IntPtr wkhtmltopdf_create_global_settings();

		[DllImport(DLL_NAME)]
		static extern bool wkhtmltopdf_set_global_setting(IntPtr globalSettings, string name, string value);

		[DllImport(DLL_NAME)]
		static extern IntPtr wkhtmltopdf_create_converter(IntPtr globalSettings);

		[DllImport(DLL_NAME)]
		static extern IntPtr wkhtmltopdf_create_object_settings();

		[DllImport(DLL_NAME)]
		// TODO: Marshal 'name' and 'value' as byte[] so we can pass UTF8Encoding.GetBytes(string) output..
		static extern bool wkhtmltopdf_set_object_setting(IntPtr objectSettings, string name, string value);

		[DllImport(DLL_NAME)]
		static extern void wkhtmltopdf_add_object(IntPtr converter, IntPtr objectSettings, string htmlData);

		[DllImport(DLL_NAME)]
		static extern bool wkhtmltopdf_convert(IntPtr converter);

		[DllImport(DLL_NAME)]
		static extern int wkhtmltopdf_get_output(IntPtr converter, out IntPtr data);

		[DllImport(DLL_NAME)]
		static extern void wkhtmltopdf_destroy_converter(IntPtr converter);

		[DllImport(DLL_NAME)]
		static extern int wkhtmltopdf_current_phase(IntPtr converter);

		[DllImport(DLL_NAME)]
		static extern int wkhtmltopdf_phase_count(IntPtr converter);

		[DllImport(DLL_NAME)]
		// NOTE: Using IntPtr as return to avoid runtime from freeing returned string. (pruiz)
		static extern IntPtr wkhtmltopdf_phase_description(IntPtr converter, int phase);

		[DllImport(DLL_NAME)]
		// NOTE: Using IntPtr as return to avoid runtime from freeing returned string. (pruiz)
		static extern IntPtr wkhtmltopdf_progress_string (IntPtr converter);

		[DllImport(DLL_NAME)]
		static extern int wkhtmltopdf_http_error_code (IntPtr converter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void wkhtmltopdf_str_callback(IntPtr converter, string str);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void wkhtmltopdf_int_callback(IntPtr converter, int val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void wkhtmltopdf_bool_callback(IntPtr converter, bool val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void wkhtmltopdf_void_callback(IntPtr converter);

		[DllImport(DLL_NAME)]
		static extern void wkhtmltopdf_set_error_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] wkhtmltopdf_str_callback cb);

		[DllImport(DLL_NAME)]
		static extern void wkhtmltopdf_set_warning_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] wkhtmltopdf_str_callback cb);

		[DllImport(DLL_NAME)]
		static extern void wkhtmltopdf_set_phase_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] wkhtmltopdf_void_callback cb);

		[DllImport(DLL_NAME)]
		static extern void wkhtmltopdf_set_progress_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] wkhtmltopdf_int_callback cb);

		[DllImport(DLL_NAME)]
		static extern void wkhtmltopdf_set_finished_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] wkhtmltopdf_bool_callback cb);
		#endregion

		#region .ctors
		static WkHtmlToPdfConverter()
		{
			// Deploy native assemblies..
			LibsHelper.DeployLibraries();
		}

		public WkHtmlToPdfConverter()
		{
			if (!wkhtmltopdf_init(0))
				throw new InvalidOperationException("wkhtmltopdf_init failed!");
		}
		#endregion

		#region Global/Object settings code..
		private IDictionary<string, object> GetProperties(string prefix, object instance)
		{
			var dict = new Dictionary<string, object>();
			var type = instance.GetType();
			var properties = type.GetProperties();

			foreach (var property in properties)
			{
				var ptype = property.PropertyType;
				var name = property.Name;
				var obj = property.GetValue(instance, null);
				var @default = ptype.IsValueType ? Activator.CreateInstance(ptype) : null;

				// Fix camel casing as used by wkhtmltopdf property names.
				name = Char.ToLower(name[0]) + name.Substring(1);

				// Prepend prefix (if any).
				name = prefix + name;

				if (ptype.IsValueType || ptype == typeof(string))
				{
					if (!object.Equals(obj, @default))
						dict.Add(name, obj);
				}
				else
				{
					foreach (var item in GetProperties(name + ".", obj))
						dict.Add(item.Key, item.Value);
				}
			}

			return dict;
		}

		#region GlobalSettings
		private void _SetGlobalSetting(IntPtr settings, string name, object value)
		{
			var tmp = value is string ? value as string : SysConvert.ToString(value, CultureInfo.InvariantCulture);

			if (!wkhtmltopdf_set_global_setting(settings, name, tmp))
			{
				var msg = string.Format("Set GlobalSetting '{0}' as '{1}': operation failed!", name, tmp);
				throw new ApplicationException(msg);
			}
		}

		private IntPtr _BuildGlobalSettings()
		{
			var ptr = wkhtmltopdf_create_global_settings();

			foreach (var item in GetProperties(null, GlobalSettings))
				_SetGlobalSetting(ptr, item.Key, item.Value);

			return ptr;
		}
		#endregion

		#region ObjectSettings
		private void _SetObjectSetting(IntPtr settings, string name, object value)
		{
			var tmp = value is string ? value as string : SysConvert.ToString(value, CultureInfo.InvariantCulture);

			if (!wkhtmltopdf_set_object_setting(settings, name, tmp))
			{
				var msg = string.Format("Set ObjectSetting '{0}' as '{1}': operation failed!", name, tmp);
				throw new ApplicationException(msg);
			}
		}

		private IntPtr _BuildObjectsettings()
		{
			var ptr = wkhtmltopdf_create_object_settings();

			foreach (var item in GetProperties(null, ObjectSettings))
				_SetObjectSetting(ptr, item.Key, item.Value);

			return ptr;
		}
		#endregion
		#endregion
		
		#region Event dispatchers.
		private void OnBegin(int expectedPhases)
		{
			try
			{
				Begin(this, new EventArgs<int>(expectedPhases));
			}
			catch (Exception ex)
			{
				_Log.Error("Begin event handler failed.", ex);
			}
		}
		private void OnError(IntPtr ptr, string error)
		{
			_errorString.AppendFormat("{0}{1}", error, Environment.NewLine);

			try
			{
				Error(this, new EventArgs<string>(error));
			}
			catch (Exception ex)
			{
				_Log.Error("Error event handler failed.", ex);
			}
		}
		private void OnWarning(IntPtr ptr, string warn)
		{
			try
			{
				Warning(this, new EventArgs<string>(warn));
			}
			catch (Exception ex)
			{
				_Log.Error("Warning event handler failed.", ex);
			}
		}
		private void OnPhaseChanged(IntPtr converter)
		{
			var tmp = wkhtmltopdf_phase_description(converter, _currentPhase);
			var str = Marshal.PtrToStringAnsi(tmp);

			try
			{
				PhaseChanged(this, new EventArgs<int, string>(++_currentPhase, str));
			}
			catch (Exception ex)
			{
				_Log.Error("PhaseChanged event handler failed.", ex);
			}
		}
		private void OnProgressChanged(IntPtr converter, int progress)
		{
			var tmp = wkhtmltopdf_progress_string(converter);
			var str = Marshal.PtrToStringAnsi(tmp);

			try
			{
				ProgressChanged(this, new EventArgs<int, string>(progress, str));
			}
			catch (Exception ex)
			{
				_Log.Error("ProgressChanged event handler failed.", ex);
			}
		}
		private void OnFinished(IntPtr converter, bool success)
		{
			try
			{
				Finished(this, new EventArgs<bool>(success));
			}
			catch (Exception ex)
			{
				_Log.Error("Finished event handler failed.", ex);
			}
		}
		#endregion

		#region
		private IntPtr _BuildConverter(IntPtr globalSettings, IntPtr objectSettings, string inputHtml)
		{
			var converter = wkhtmltopdf_create_converter(globalSettings);
			wkhtmltopdf_add_object(converter, objectSettings, inputHtml);

			return converter;
		}

		private byte[] _Convert(string inputHtml)
		{
			IntPtr converter = IntPtr.Zero;
			var errorCb = new wkhtmltopdf_str_callback(OnError);
			var warnCb = new wkhtmltopdf_str_callback(OnWarning);
			var phaseCb = new wkhtmltopdf_void_callback(OnPhaseChanged);
			var progressCb = new wkhtmltopdf_int_callback(OnProgressChanged);
			var finishCb = new wkhtmltopdf_bool_callback(OnFinished);

			try
			{
				var gSettings = _BuildGlobalSettings();
				var oSettings = _BuildObjectsettings();
				converter = _BuildConverter(gSettings, oSettings, inputHtml);

				_errorString = new StringBuilder();
				
				wkhtmltopdf_set_error_callback(converter, errorCb);
				wkhtmltopdf_set_warning_callback(converter, warnCb);
				wkhtmltopdf_set_phase_changed_callback(converter, phaseCb);
				wkhtmltopdf_set_progress_changed_callback(converter, progressCb);
				wkhtmltopdf_set_finished_callback(converter, finishCb);

				OnBegin(wkhtmltopdf_phase_count(converter));

				if (!wkhtmltopdf_convert(converter))
				{
					var msg = string.Format("HtmlToPdf conversion failed: {0}", _errorString.ToString());
					throw new ApplicationException(msg);
				}

				if (!string.IsNullOrEmpty(GlobalSettings.Out))
					return null;

				_Log.Debug("CONVERSION DONE.. getting output.");

				// Get output from internal buffer..

				IntPtr tmp = IntPtr.Zero;
				var len = wkhtmltopdf_get_output(converter, out tmp);
				var output = new byte[len];
				Marshal.Copy(tmp, output, 0, output.Length);

				return output;
			}
			finally
			{
				if (converter != IntPtr.Zero)
				{
					wkhtmltopdf_set_error_callback(converter, null);
					wkhtmltopdf_set_warning_callback(converter, null);
					wkhtmltopdf_set_phase_changed_callback(converter, null);
					wkhtmltopdf_set_progress_changed_callback(converter, null);
					wkhtmltopdf_set_finished_callback(converter, null);
					wkhtmltopdf_destroy_converter(converter);
				}
			}
		}

		public byte[] Convert()
		{
			if (string.IsNullOrEmpty(ObjectSettings.Page))
				throw new InvalidOperationException("You must specify a web page to convert (using ObjectSettings.Page)");

			return _Convert(null);
		}

		public byte[] Convert(string inputHtml)
		{
			if (inputHtml == null)
				throw new ArgumentNullException("inputHtml");

			return _Convert(inputHtml);
		}
		#endregion

		#region IDisposable
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				_Log.Warn("Disposed was called more than once?!");
				return;
			}

			if (disposing)
			{
				// Dispose managed resources..
				Begin = null;
				PhaseChanged = null;
				ProgressChanged = null;
				Finished = null;
				Error = null;
				Warning = null;
			}

			// Dispose un-managed resources..
			wkhtmltopdf_deinit();
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~WkHtmlToPdfConverter()
		{
			Dispose(false);
		}
		#endregion
	}
}
