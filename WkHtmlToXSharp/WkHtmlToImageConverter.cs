#region Copyright
//
// Author: Dan Ambrisco (dan@dambrisco.com)
//
// (C) Dan Ambrisco 2012
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
    public class WkHtmlToImageConverter : WkHtmlToXConverter, IHtmlToImageConverter
    {
        #region private fields
<<<<<<< HEAD
        private static readonly global::Common.Logging.ILog _Log = global::Common.Logging.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string DLL_NAME = "wkhtmltox0";
        private ImageGlobalSettings _globalSettings = new ImageGlobalSettings()
        {
            Fmt = "PNG",
            SmartWidth = false
=======
        private readonly ImageGlobalSettings _globalSettings = new ImageGlobalSettings() {
            Fmt = "PNG"
>>>>>>> dca205949928617a22e3e0258c28f3736ce96915
        };
        #endregion

        #region Properties

        public override IGlobalSettings GlobalSettings { get { return _globalSettings; } }
        public override IObjectSettings ObjectSettings { get { return null; } }

        public ImageGlobalSettings ImageGlobalSettings { get { return _globalSettings; } }

        #endregion

        #region P/Invokes

        [DllImport(DLL_NAME)]
        static extern IntPtr wkhtmltoimage_version();
        protected override IntPtr WkHtmlToXVersion()
        {
            return wkhtmltoimage_version();
        }

        [DllImport(DLL_NAME)]
        static extern bool wkhtmltoimage_init(int useGraphics);
        protected override bool WkHtmlToXInit(int useGraphics)
        {
            return wkhtmltoimage_init(useGraphics);
        }

        [DllImport(DLL_NAME)]
        static extern bool wkhtmltoimage_deinit();
        protected override bool WkHtmlToXDeInit()
        {
            return wkhtmltoimage_deinit();
        }

        [DllImport(DLL_NAME)]
        static extern IntPtr wkhtmltoimage_create_global_settings();
        protected override IntPtr WkHtmlToXCreateGlobalSettings()
        {
            return wkhtmltoimage_create_global_settings();
        }

        [DllImport(DLL_NAME)]
        static extern bool wkhtmltoimage_set_global_setting(IntPtr globalSettings, string name, string value);
        protected override bool WkHtmlToXSetGlobalSetting(IntPtr globalSettings, string name, string value)
        {
            return wkhtmltoimage_set_global_setting(globalSettings, name, value);
        }

        [DllImport(DLL_NAME)]
        // data is inputHtml
        static extern IntPtr wkhtmltoimage_create_converter(IntPtr globalSettings, IntPtr htmlData);

        [DllImport(DLL_NAME)]
        static extern IntPtr wkhtmltoimage_create_object_settings();
        protected override IntPtr WkHtmlToXCreateObjectSettings()
        {
            return wkhtmltoimage_create_object_settings();
        }

        [DllImport(DLL_NAME)]
        // TODO: Marshal 'name' and 'value' as byte[] so we can pass UTF8Encoding.GetBytes(string) output..
        static extern bool wkhtmltoimage_set_object_setting(IntPtr objectSettings, string name, string value);
        protected override bool WkHtmlToXSetObjectSetting(IntPtr objectSettings, string name, string value)
        {
            return wkhtmltoimage_set_object_setting(objectSettings, name, value);
        }

        [DllImport(DLL_NAME)]
        static extern bool wkhtmltoimage_convert(IntPtr converter);
        protected override bool WkHtmlToXConvert(IntPtr converter)
        {
            return wkhtmltoimage_convert(converter);
        }

        [DllImport(DLL_NAME)]
        static extern int wkhtmltoimage_get_output(IntPtr converter, out IntPtr data);
        protected override int WkHtmlToXGetOutput(IntPtr converter, out IntPtr data)
        {
            return wkhtmltoimage_get_output(converter, out  data);
        }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltoimage_destroy_converter(IntPtr converter);
        protected override void WkHtmlToXDestroyConverter(IntPtr converter)
        {
            wkhtmltoimage_destroy_converter(converter);
        }

        [DllImport(DLL_NAME)]
        static extern int wkhtmltoimage_current_phase(IntPtr converter);
        protected override int WkHtmlToXCurrentPhase(IntPtr converter)
        {
            return wkhtmltoimage_current_phase(converter);
        }

        [DllImport(DLL_NAME)]
        static extern int wkhtmltoimage_phase_count(IntPtr converter);
        protected override int WkHtmlToXPhaseCount(IntPtr converter)
        {
            return wkhtmltoimage_phase_count(converter);
        }

        [DllImport(DLL_NAME)]
        // NOTE: Using IntPtr as return to avoid runtime from freeing returned string. (pruiz)
        static extern IntPtr wkhtmltoimage_phase_description(IntPtr converter, int phase);
        protected override IntPtr WkHtmlToXPhaseDescription(IntPtr converter, int phase)
        {
            return wkhtmltoimage_phase_description(converter, phase);
        }

        [DllImport(DLL_NAME)]
        // NOTE: Using IntPtr as return to avoid runtime from freeing returned string. (pruiz)
        static extern IntPtr wkhtmltoimage_progress_string(IntPtr converter);
        protected override IntPtr WkHtmlToXProgressString(IntPtr converter)
        {
            return wkhtmltoimage_progress_string(converter);
        }

        [DllImport(DLL_NAME)]
        static extern int wkhtmltoimage_http_error_code(IntPtr converter);
        protected override int WkHtmlToXHttpErrorCode(IntPtr converter)
        {
            return wkhtmltoimage_http_error_code(converter);
        }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltoimage_set_error_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXStrCallback cb);
        protected override void WkHtmlToXSetErrorCallback(IntPtr converter, WkHtmlToXStrCallback cb)
        {
            wkhtmltoimage_set_error_callback(converter, cb);
        }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltoimage_set_warning_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXStrCallback cb);
        protected override void WkHtmlToXSetWarningCallback(IntPtr converter, WkHtmlToXStrCallback cb)
        {
            wkhtmltoimage_set_warning_callback(converter, cb);
        }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltoimage_set_phase_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXVoidCallback cb);
        protected override void WkHtmlToXSetPhaseChangedCallback(IntPtr converter, WkHtmlToXVoidCallback cb)
        {
            wkhtmltoimage_set_phase_changed_callback(converter, cb);
        }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltoimage_set_progress_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXIntCallback cb);
        protected override void WkHtmlToXSetProgressChangedCallback(IntPtr converter, WkHtmlToXIntCallback cb)
        {
            wkhtmltoimage_set_progress_changed_callback(converter, cb);
        }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltoimage_set_finished_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXBoolCallback cb);
        protected override void WkHtmlToXSetFinishedCallback(IntPtr converter, WkHtmlToXBoolCallback cb)
        {
            wkhtmltoimage_set_finished_callback(converter, cb);
        }

        #endregion

        #region Conversion methods

        protected override IntPtr _BuildConverter(IntPtr globalSettings, IntPtr objectSettings, IntPtr inputHtml)
        {
            var converter = wkhtmltoimage_create_converter(globalSettings, inputHtml);
            return converter;
        }

        // we don't have object settings for the images
        protected override IntPtr _BuildObjectsettings()
        {
            return IntPtr.Zero;
        }

        #endregion
    }
}
