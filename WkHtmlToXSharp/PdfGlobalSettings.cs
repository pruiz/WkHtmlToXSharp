using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WkHtmlToXSharp
{
	public class PdfGlobalSettings
	{
		private PdfMarginSettings _margins = new PdfMarginSettings();

		public int Dpi { get; set; }
		public int ImageDpi { get; set; }
		public int ImageQuality { get; set; }
		public PdfMarginSettings Margin { get { return _margins; } }

		public string Out { get; set; }

		// TODO: Add as many as you need..
	}
}
