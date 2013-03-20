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
using System.Runtime.InteropServices;

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
	public sealed class WkHtmlToPdfConverter : WkHtmlToXConverter, IHtmlToPdfConverter
	{
		#region private fields
		private readonly PdfGlobalSettings _globalSettings = new PdfGlobalSettings();
		private readonly PdfObjectSettings _objectSettings = new PdfObjectSettings();
		#endregion

		#region Properties

	    public override IGlobalSettings GlobalSettings { get { return _globalSettings; } }
	    public override IObjectSettings ObjectSettings { get { return _objectSettings; } }

        public PdfObjectSettings PdfObjectSettings { get { return _objectSettings; } }
        public PdfGlobalSettings PdfGlobalSettings { get { return _globalSettings; } }

		#endregion

		#region P/Invokes

		[DllImport(DLL_NAME)]
		static extern IntPtr wkhtmltopdf_version();
	    protected override IntPtr WkHtmlToXVersion() {
	        return wkhtmltopdf_version();
	    }

        [DllImport(DLL_NAME)]
        static extern bool wkhtmltopdf_init(int useGraphics);
        protected override bool WkHtmlToXInit(int useGraphics) {
            return wkhtmltopdf_init(useGraphics);
        }

        [DllImport(DLL_NAME)]
        static extern bool wkhtmltopdf_deinit();
	    protected override bool WkHtmlToXDeInit(){
	        return wkhtmltopdf_deinit();
	    }

        [DllImport(DLL_NAME)]
        static extern IntPtr wkhtmltopdf_create_global_settings();
	    protected override IntPtr WkHtmlToXCreateGlobalSettings() {
            return wkhtmltopdf_create_global_settings();
	    }

        [DllImport(DLL_NAME)]
        static extern bool wkhtmltopdf_set_global_setting(IntPtr globalSettings, string name, string value);
	    protected override bool WkHtmlToXSetGlobalSetting(IntPtr globalSettings, string name, string value) {
            return wkhtmltopdf_set_global_setting(globalSettings, name, value);
	    }

        [DllImport(DLL_NAME)]
        static extern IntPtr wkhtmltopdf_create_converter(IntPtr globalSettings);

        [DllImport(DLL_NAME)]
        static extern IntPtr wkhtmltopdf_create_object_settings();
	    protected override IntPtr WkHtmlToXCreateObjectSettings() {
            return wkhtmltopdf_create_object_settings();
	    }

        [DllImport(DLL_NAME)]
        // TODO: Marshal 'name' and 'value' as byte[] so we can pass UTF8Encoding.GetBytes(string) output..
        static extern bool wkhtmltopdf_set_object_setting(IntPtr objectSettings, string name, string value);
	    protected override bool WkHtmlToXSetObjectSetting(IntPtr objectSettings, string name, string value) {
            return wkhtmltopdf_set_object_setting(objectSettings, name, value);
	    }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltopdf_add_object(IntPtr converter, IntPtr objectSettings, IntPtr htmlData);

        [DllImport(DLL_NAME)]
        static extern bool wkhtmltopdf_convert(IntPtr converter);
	    protected override bool WkHtmlToXConvert(IntPtr converter) {
            return wkhtmltopdf_convert(converter);
	    }

        [DllImport(DLL_NAME)]
        static extern int wkhtmltopdf_get_output(IntPtr converter, out IntPtr data);
	    protected override int WkHtmlToXGetOutput(IntPtr converter, out IntPtr data)
	    {
            return wkhtmltopdf_get_output(converter, out  data);
	    }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltopdf_destroy_converter(IntPtr converter);
	    protected override void WkHtmlToXDestroyConverter(IntPtr converter)
	    {
            wkhtmltopdf_destroy_converter(converter);
	    }

        [DllImport(DLL_NAME)]
        static extern int wkhtmltopdf_current_phase(IntPtr converter);
	    protected override int WkHtmlToXCurrentPhase(IntPtr converter)
	    {
            return wkhtmltopdf_current_phase(converter);
	    }

        [DllImport(DLL_NAME)]
        static extern int wkhtmltopdf_phase_count(IntPtr converter);
	    protected override int WkHtmlToXPhaseCount(IntPtr converter)
	    {
            return wkhtmltopdf_phase_count(converter);
	    }

        [DllImport(DLL_NAME)]
        // NOTE: Using IntPtr as return to avoid runtime from freeing returned string. (pruiz)
        static extern IntPtr wkhtmltopdf_phase_description(IntPtr converter, int phase);
	    protected override IntPtr WkHtmlToXPhaseDescription(IntPtr converter, int phase)
	    {
            return wkhtmltopdf_phase_description(converter, phase);
	    }

        [DllImport(DLL_NAME)]
        // NOTE: Using IntPtr as return to avoid runtime from freeing returned string. (pruiz)
        static extern IntPtr wkhtmltopdf_progress_string(IntPtr converter);
	    protected override IntPtr WkHtmlToXProgressString(IntPtr converter)
	    {
            return wkhtmltopdf_progress_string(converter);
	    }

        [DllImport(DLL_NAME)]
        static extern int wkhtmltopdf_http_error_code(IntPtr converter);
	    protected override int WkHtmlToXHttpErrorCode(IntPtr converter)
	    {
            return wkhtmltopdf_http_error_code(converter);
	    }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltopdf_set_error_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXStrCallback cb);
        protected override void WkHtmlToXSetErrorCallback(IntPtr converter, WkHtmlToXStrCallback cb)
        {
            wkhtmltopdf_set_error_callback(converter, cb);
        }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltopdf_set_warning_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXStrCallback cb);
        protected override void WkHtmlToXSetWarningCallback(IntPtr converter, WkHtmlToXStrCallback cb)
        {
            wkhtmltopdf_set_warning_callback(converter, cb);
        }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltopdf_set_phase_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXVoidCallback cb);
        protected override void WkHtmlToXSetPhaseChangedCallback(IntPtr converter, WkHtmlToXVoidCallback cb) {
            wkhtmltopdf_set_phase_changed_callback(converter, cb);
        }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltopdf_set_progress_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXIntCallback cb);
        protected override void WkHtmlToXSetProgressChangedCallback(IntPtr converter, WkHtmlToXIntCallback cb) {
            wkhtmltopdf_set_progress_changed_callback(converter, cb);
        }

        [DllImport(DLL_NAME)]
        static extern void wkhtmltopdf_set_finished_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] WkHtmlToXBoolCallback cb);
        protected override void WkHtmlToXSetFinishedCallback(IntPtr converter, WkHtmlToXBoolCallback cb) {
            wkhtmltopdf_set_finished_callback(converter, cb);
        }

		#endregion

        #region Conversion methods

        protected override IntPtr _BuildConverter(IntPtr globalSettings, IntPtr objectSettings, IntPtr inputHtml)
        {
            var converter = wkhtmltopdf_create_converter(globalSettings);
            wkhtmltopdf_add_object(converter, objectSettings, inputHtml);
            return converter;
        }

        #endregion
    }

}
