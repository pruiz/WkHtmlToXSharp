using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WkHtmlToXSharp
{
	public class EventArgs<T> : EventArgs
	{
		public EventArgs(T value)
		{
			m_value = value;
		}

		private T m_value;

		public T Value
		{
			get { return m_value; }
		}
	}

	public class EventArgs<T1, T2> : EventArgs<T1>
	{
		public T2 Value2 { get; private set; }

		public EventArgs(T1 value, T2 value2)
			: base(value)
		{
			Value2 = value2;
		}
	}
}
