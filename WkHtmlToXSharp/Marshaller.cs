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
