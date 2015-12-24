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
using System.Linq;
using System.Text;

namespace WkHtmlToXSharp
{
	public enum PdfOrientation {
		Portrait,
		Landscape
	}

	public enum PdfPageSize
	{
		A4, B5, Letter, Legal, Executive, A0, A1, A2, A3, A5, A6, A7,
		A8, A9, B0, B1, B10, B2, B3, B4, B6, B7, B8, B9, C5E,
		Comm10E, DLE, Folio, Ledger, Tabloid, Custom
	}

	public class PdfSize
	{
		/// <summary>
		/// What size paper should we use
		/// </summary>
		public PdfPageSize PageSize { get; set; }
	}

	public class PdfGlobalSettings
	{
		private readonly PdfMarginSettings _margins = new PdfMarginSettings();
		private readonly PdfSize _size = new PdfSize()
		{
			PageSize = PdfPageSize.A4
		};

		public int Dpi { get; set; }
		public int ImageDpi { get; set; }
		public int ImageQuality { get; set; }
		public PdfMarginSettings Margin { get { return _margins; } }

		/// <summary>
		/// Should we generate an outline and put it into the pdf file
		/// </summary>
		public bool Outline { get; set; }

		/// <summary>
		/// The file where in to store the output
		/// </summary>
		public string Out { get; set; }
		public PdfOrientation Orientation { get; set; }

		/// <summary>
		/// Size related settings
		/// </summary>
		public PdfSize Size { get { return _size; } }

		// TODO: Add as many as you need..
	}
}
