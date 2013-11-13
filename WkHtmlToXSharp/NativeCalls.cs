using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WkHtmlToXSharp
{
	internal static class NativeCalls
	{
		private const string DLL_NAME = "wkhtmltox0";

		static NativeCalls()
		{
			// Deploy native assemblies..
			LibsHelper.DeployLibraries();
		}

		#region P/Invokes
		[DllImport(DLL_NAME)]
		public static extern IntPtr wkhtmltopdf_version();

		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_init(int use_graphics);

		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_deinit();

		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_extended_qt();

		[DllImport(DLL_NAME)]
		public static extern IntPtr wkhtmltopdf_create_global_settings();

		[DllImport(DLL_NAME)]
		public static extern IntPtr wkhtmltopdf_create_converter(IntPtr globalSettings);

		[DllImport(DLL_NAME)]
		public static extern IntPtr wkhtmltopdf_create_object_settings();

#if true
		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_set_global_setting(IntPtr globalSettings,
			[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Marshaler))] string name,
			[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Marshaler))] string value);

		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_set_object_setting(IntPtr objectSettings,
			[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Marshaler))] string name,
			[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Marshaler))] string value);

		[DllImport(DLL_NAME)]
		public static extern void wkhtmltopdf_add_object(IntPtr converter, IntPtr objectSettings,
			[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Marshaler))] string htmlData);
#else
		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_set_global_setting(IntPtr globalSettings, string name, IntPtr value);

		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_set_object_setting(IntPtr objectSettings, string name, IntPtr value);

		[DllImport(DLL_NAME)]
		public static extern void wkhtmltopdf_add_object(IntPtr converter, IntPtr objectSettings, IntPtr htmlData);
#endif

		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_convert(IntPtr converter);

		[DllImport(DLL_NAME)]
		public static extern IntPtr wkhtmltopdf_get_output(IntPtr converter, out IntPtr data);

		[DllImport(DLL_NAME)]
		public static extern void wkhtmltopdf_destroy_converter(IntPtr converter);

		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_current_phase(IntPtr converter);

		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_phase_count(IntPtr converter);

		[DllImport(DLL_NAME)]
		// NOTE: Using IntPtr as return to avoid runtime from freeing returned string. (pruiz)
		public static extern IntPtr wkhtmltopdf_phase_description(IntPtr converter, int phase);

		[DllImport(DLL_NAME)]
		// NOTE: Using IntPtr as return to avoid runtime from freeing returned string. (pruiz)
		public static extern IntPtr wkhtmltopdf_progress_string(IntPtr converter);

		[DllImport(DLL_NAME)]
		public static extern int wkhtmltopdf_http_error_code(IntPtr converter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void wkhtmltopdf_str_callback(IntPtr converter, [MarshalAs(UnmanagedType.LPStr)] string str);
		//public delegate void wkhtmltopdf_str_callback(IntPtr converter, IntPtr str);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void wkhtmltopdf_int_callback(IntPtr converter, int val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void wkhtmltopdf_bool_callback(IntPtr converter, bool val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void wkhtmltopdf_void_callback(IntPtr converter);

		[DllImport(DLL_NAME)]
		public static extern void wkhtmltopdf_set_error_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] wkhtmltopdf_str_callback cb);

		[DllImport(DLL_NAME)]
		public static extern void wkhtmltopdf_set_warning_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] wkhtmltopdf_str_callback cb);

		[DllImport(DLL_NAME)]
		public static extern void wkhtmltopdf_set_phase_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] wkhtmltopdf_void_callback cb);

		[DllImport(DLL_NAME)]
		public static extern void wkhtmltopdf_set_progress_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] wkhtmltopdf_int_callback cb);

		[DllImport(DLL_NAME)]
		public static extern void wkhtmltopdf_set_finished_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] wkhtmltopdf_bool_callback cb);
		#endregion

		#region NativeCall Wrappers
		public static string WkHtmlToPdfVersion()
		{
			var ptr = wkhtmltopdf_version();
			return Marshal.PtrToStringAnsi(ptr);
		}
		#endregion
	}
}
