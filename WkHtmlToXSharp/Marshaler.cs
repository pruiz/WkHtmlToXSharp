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
	/// <summary>
	/// Marshaller needed to correctly pass values between us and QT APIs.
	/// </summary>
	public class Marshaler : ICustomMarshaler
	{
		public static readonly Marshaler Instance = new Marshaler();

		#region Helper Methods
		public static IntPtr StringToUtf8Ptr(string str)
		{
			return Instance.MarshalManagedToNative(str);
		}

		public static void FreeUtf8Ptr(IntPtr ptr)
		{
			Instance.CleanUpNativeData(ptr);
		}
		#endregion

		#region ICusomMarshaler
		public IntPtr MarshalManagedToNative(object obj)
		{
			if (obj == null)
				return IntPtr.Zero;

			if (!(obj is string))
				throw new MarshalDirectiveException("Object must be a string.");

			// not null terminated
			byte[] strbuf = Encoding.UTF8.GetBytes((string)obj);
			var buffer = Marshal.AllocHGlobal(strbuf.Length + 1);
			Marshal.Copy(strbuf, 0, buffer, strbuf.Length);

			// append final null
			Marshal.WriteByte(buffer, strbuf.Length, 0);

			return buffer;
		}

		public unsafe object MarshalNativeToManaged(IntPtr ptr)
		{
			byte* walk = (byte*)ptr;

			// find the end of the string
			while (*walk != 0)
			{
				walk++;
			}
			int length = (int)(walk - (byte*)ptr);

			// should not be null terminated
			byte[] strbuf = new byte[length];

			// skip the trailing null
			Marshal.Copy(ptr, strbuf, 0, length);
			return Encoding.UTF8.GetString(strbuf);
		}

		public void CleanUpNativeData(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return;

			Marshal.FreeHGlobal(ptr);
		}

		public void CleanUpManagedData(object managedObj)
		{
		}

		public int GetNativeDataSize()
		{
			return -1;
		}

		public static ICustomMarshaler GetInstance(string cookie)
		{
			return Instance;
		}
		#endregion
	}
}
