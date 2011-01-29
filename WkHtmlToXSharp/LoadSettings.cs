using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WkHtmlToXSharp
{
	public class LoadSettings
	{
		public bool UserName { get; set; }
		public bool Password { get; set; }
		public bool BlockLocalFileAccess { get; set; }
		public string Proxy { get; set; }

		// TODO: Add remaining settings..

	}
}
