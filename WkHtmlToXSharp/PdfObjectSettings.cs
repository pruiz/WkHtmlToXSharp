using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WkHtmlToXSharp
{
	public class PdfObjectSettings
	{
		private WebSettings _webSettings = new WebSettings();
		private LoadSettings _loadSettings = new LoadSettings();

		public string Page { get; set; }

		public WebSettings Web { get { return _webSettings; } }
		public LoadSettings Load { get { return _loadSettings; } }

		// TODO: Add remaining settings..
	}
}
