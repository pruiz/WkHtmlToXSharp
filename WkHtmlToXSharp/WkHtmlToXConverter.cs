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
using System.Configuration;
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
    /// WARNING: Due to underlaying's API restrictions all calls to
    /// instances of this class should be made from within the same thread!!
    /// See MultiplexingConverter for an interim & transparent solution.
    /// </remarks>
    public abstract class WkHtmlToXConverter : IHtmlToXConverter
    {
        #region Protected Fields
        protected static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected const string DLL_NAME = "wkhtmltox0";
        protected StringBuilder _errorString = null;
        protected int _currentPhase = 0;
        protected bool _disposed = false;
        #endregion

        #region Events
        public event EventHandler<EventArgs<int>> Begin = delegate { };
        public event EventHandler<EventArgs<int, string>> PhaseChanged = delegate { };
        public event EventHandler<EventArgs<int, string>> ProgressChanged = delegate { };
        public event EventHandler<EventArgs<bool>> Finished = delegate { };
        public event EventHandler<EventArgs<string>> Error = delegate { };
        public event EventHandler<EventArgs<string>> Warning = delegate { };
        #endregion

        #region Abstract Methods

        protected abstract IntPtr WkHtmlToXVersion();

        protected abstract bool WkHtmlToXInit(int useGraphics);

        protected abstract bool WkHtmlToXDeInit();

        protected abstract IntPtr WkHtmlToXCreateGlobalSettings();

        protected abstract bool WkHtmlToXSetGlobalSetting(IntPtr globalSettings, string name, string value);

        //protected abstract IntPtr WkHtmlToXCreateConverter(IntPtr globalSettings);

        protected abstract IntPtr WkHtmlToXCreateObjectSettings();

        protected abstract bool WkHtmlToXSetObjectSetting(IntPtr objectSettings, string name, string value);

        //protected abstract void WkHtmlToXAddObject(IntPtr converter, IntPtr objectSettings, IntPtr htmlData);

        protected abstract bool WkHtmlToXConvert(IntPtr converter);

        protected abstract int WkHtmlToXGetOutput(IntPtr converter, out IntPtr data);

        protected abstract void WkHtmlToXDestroyConverter(IntPtr converter);

        protected abstract int WkHtmlToXCurrentPhase(IntPtr converter);

        protected abstract int WkHtmlToXPhaseCount(IntPtr converter);

        protected abstract IntPtr WkHtmlToXPhaseDescription(IntPtr converter, int phase);

        protected abstract IntPtr WkHtmlToXProgressString(IntPtr converter);

        protected abstract int WkHtmlToXHttpErrorCode(IntPtr converter);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void WkHtmlToXStrCallback(IntPtr converter, string str);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void WkHtmlToXIntCallback(IntPtr converter, int val);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void WkHtmlToXBoolCallback(IntPtr converter, bool val);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void WkHtmlToXVoidCallback(IntPtr converter);

        protected abstract void WkHtmlToXSetErrorCallback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXStrCallback cb);

        
        protected abstract void WkHtmlToXSetWarningCallback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXStrCallback cb);

        
        protected abstract void WkHtmlToXSetPhaseChangedCallback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXVoidCallback cb);

        
        protected abstract void WkHtmlToXSetProgressChangedCallback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXIntCallback cb);

        
        protected abstract void WkHtmlToXSetFinishedCallback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXBoolCallback cb);
        #endregion

        #region Abstract Properties 

        public abstract IGlobalSettings GlobalSettings { get; }
        public abstract IObjectSettings ObjectSettings { get; }

        #endregion

        #region .ctors
        static WkHtmlToXConverter()
        {
            // Deploy native assemblies..
            LibsHelper.DeployLibraries();
        }

        protected WkHtmlToXConverter()
        {
            bool useX11 = false;

            try
            {
                useX11 = SysConvert.ToBoolean(ConfigurationManager.AppSettings["WkHtmlToXSharp.UseX11"]);
            }
            catch (Exception ex)
            {
                _Log.Error("Unable to parse 'WkHtmlToXSharp.UseX11' app. setting.", ex);
            }

            var ptr = WkHtmlToXVersion();
            var version = Marshal.PtrToStringAnsi(ptr);

            if (!WkHtmlToXInit(useX11 ? 1 : 0))
                throw new InvalidOperationException(string.Format("WkHtmlToXInit failed! (version: {0}, useX11 = {1})", version, useX11));

            _Log.DebugFormat("Initialized new converter instance (Version: {0}, UseX11 = {1})", version, useX11);
        }

        #endregion

        #region Global/Object settings code..
        protected IDictionary<string, object> GetProperties(string prefix, object instance)
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
        protected void _SetGlobalSetting(IntPtr settings, string name, object value)
        {
            var tmp = GetStringValue(value);

            if (!WkHtmlToXSetGlobalSetting(settings, name, tmp))
            {
                var msg = string.Format("Set GlobalSetting '{0}' as '{1}': operation failed!", name, tmp);
                throw new ApplicationException(msg);
            }
        }

        protected IntPtr _BuildGlobalSettings()
        {
            var ptr = WkHtmlToXCreateGlobalSettings();

            foreach (var item in GetProperties(null, GlobalSettings))
                _SetGlobalSetting(ptr, item.Key, item.Value);

            return ptr;
        }
        #endregion

        protected string GetStringValue(object value)
        {
            var tmp = value is string ? value as string : SysConvert.ToString(value, CultureInfo.InvariantCulture);
            // Correct for differences between C booleans and C# booleans
            tmp = tmp == "True" ? "true" : tmp;
            tmp = tmp == "False" ? "false" : tmp;
            return tmp;
        }

        #region ObjectSettings
        protected void _SetObjectSetting(IntPtr settings, string name, object value)
        {
            var tmp = GetStringValue(value);

            if (!WkHtmlToXSetObjectSetting(settings, name, tmp))
            {
                var msg = string.Format("Set ObjectSetting '{0}' as '{1}': operation failed!", name, tmp);
                throw new ApplicationException(msg);
            }
        }

        protected virtual IntPtr _BuildObjectsettings()
        {
            var ptr = WkHtmlToXCreateObjectSettings();

            foreach (var item in GetProperties(null, ObjectSettings))
                _SetObjectSetting(ptr, item.Key, item.Value);

            return ptr;
        }
        #endregion
        #endregion

        #region Event dispatchers.
        protected void OnBegin(int expectedPhases)
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
        protected void OnError(IntPtr ptr, string error)
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
        protected void OnWarning(IntPtr ptr, string warn)
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
        protected void OnPhaseChanged(IntPtr converter)
        {
            var tmp = WkHtmlToXPhaseDescription(converter, _currentPhase);
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
        protected void OnProgressChanged(IntPtr converter, int progress)
        {
            var tmp = WkHtmlToXProgressString(converter);
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
        protected void OnFinished(IntPtr converter, bool success)
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

        #region Conversion methods

        protected abstract IntPtr _BuildConverter(IntPtr globalSettings, IntPtr objectSettings, IntPtr inputHtml);

        protected byte[] _Convert(string inputHtml)
        {
            var converter = IntPtr.Zero;
            var inputHtmlUtf8Ptr = IntPtr.Zero;
            var errorCb = new WkHtmlToXStrCallback(OnError);
            var warnCb = new WkHtmlToXStrCallback(OnWarning);
            var phaseCb = new WkHtmlToXVoidCallback(OnPhaseChanged);
            var progressCb = new WkHtmlToXIntCallback(OnProgressChanged);
            var finishCb = new WkHtmlToXBoolCallback(OnFinished);

            try
            {
                var gSettings = _BuildGlobalSettings();
                var oSettings = _BuildObjectsettings();

                inputHtmlUtf8Ptr = Marshaller.StringToUtf8Ptr(inputHtml);
                converter = _BuildConverter(gSettings, oSettings, inputHtmlUtf8Ptr);

                _errorString = new StringBuilder();

                WkHtmlToXSetErrorCallback(converter, errorCb);
                WkHtmlToXSetWarningCallback(converter, warnCb);
                WkHtmlToXSetPhaseChangedCallback(converter, phaseCb);
                WkHtmlToXSetProgressChangedCallback(converter, progressCb);
                WkHtmlToXSetFinishedCallback(converter, finishCb);

                OnBegin(WkHtmlToXPhaseCount(converter));

                if (!WkHtmlToXConvert(converter))
                {
                    var msg = string.Format("Html conversion failed: {0}", _errorString);
                    throw new ApplicationException(msg);
                }

                if (!string.IsNullOrEmpty(GlobalSettings.Out))
                    return null;

                _Log.Debug("CONVERSION DONE.. getting output.");

                // Get output from internal buffer..

                IntPtr tmp = IntPtr.Zero;
                var len = WkHtmlToXGetOutput(converter, out tmp);
                var output = new byte[len];
                Marshal.Copy(tmp, output, 0, output.Length);

                return output;
            }
            finally
            {
                if (converter != IntPtr.Zero)
                {
                    WkHtmlToXSetErrorCallback(converter, null);
                    WkHtmlToXSetWarningCallback(converter, null);
                    WkHtmlToXSetPhaseChangedCallback(converter, null);
                    WkHtmlToXSetProgressChangedCallback(converter, null);
                    WkHtmlToXSetFinishedCallback(converter, null);
                    WkHtmlToXDestroyConverter(converter);
                }

                if (inputHtmlUtf8Ptr != IntPtr.Zero)
                {
                    Marshaller.FreeUtf8Ptr(inputHtmlUtf8Ptr);
                }
            }
        }

        public byte[] Convert()
        {
            if (ObjectSettings != null && string.IsNullOrEmpty(ObjectSettings.Page))
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
        protected void Dispose(bool disposing)
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
            try
            {
                WkHtmlToXDeInit();
            }
            catch (DllNotFoundException)
            {
                // We may not be initialized yet
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WkHtmlToXConverter()
        {
            Dispose(false);
        }
        #endregion
    }
}
