using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WkHtmlToXSharp
{
	// See: http://www.codeproject.com/KB/cs/pinvokeStringMarshaling.aspx
	public static class Marshaller
	{
#if false
		private static unsafe ulong strlen (IntPtr s)
		{
			ulong cnt = 0;
			byte *b = (byte *)s;
			while (*b != 0) {
				b++;
				cnt++;
			}
			return cnt;
		}

		public static string Utf8PtrToString(this Marshal @this, IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return null;

			int len = (int)(uint)strlen(ptr);
			byte[] bytes = new byte[len];
			Marshal.Copy(ptr, bytes, 0, len);
			return System.Text.Encoding.UTF8.GetString(bytes);
		}
#endif

		public static IntPtr StringToUtf8Ptr(string str)
		{
			if (str == null)
				return IntPtr.Zero;

			// remember the byte[] is not null terminated
			byte[] strbuf = Encoding.UTF8.GetBytes(str);

			// .. so add one more byte for the null termination
			var buffer = Marshal.AllocHGlobal(strbuf.Length + 1);

			// .. then copy the bytes
			Marshal.Copy(strbuf, 0, buffer, strbuf.Length);
			Marshal.WriteByte(buffer, strbuf.Length, 0); // terminating null

			return buffer;
		}

		public static void FreeUtf8Ptr(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return;

			Marshal.FreeHGlobal(ptr);
		}
	}
}
