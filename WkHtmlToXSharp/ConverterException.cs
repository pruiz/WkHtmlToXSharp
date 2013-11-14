using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WkHtmlToXSharp
{
	[Serializable]
	public class ConverterException : Exception
	{
		public ConverterException() { }
		public ConverterException(string message) : base(message) { }
		public ConverterException(string message, Exception inner) : base(message, inner) { }
		protected ConverterException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
