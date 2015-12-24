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

namespace WkHtmlToXSharp
{
	public class HeaderFooterSettings
	{
		private int _fontSize = -1;
		

		/// <summary>
		/// The font size to use for the header, e.g. "13"
		/// </summary>
		public int FontSize
		{
			get { return _fontSize; }
			set { _fontSize = value; }
		}

		private string _fontName = string.Empty;

		/// <summary>
		/// The name of the font to use for the header. e.g. "times"
		/// </summary>
		public string FontName
		{
			get { return _fontName; }
			set { _fontName = value; }
		}

		private string _left = string.Empty;

		/// <summary>
		/// The string to print in the left part of the header, note that some sequences are replaced in this string, see the wkhtmltopdf manual.
		/// </summary>
		public string Left
		{
			get { return _left; }
			set { _left = value; }
		}

		private string _center = string.Empty;

		/// <summary>
		/// The text to print in the center part of the header.
		/// </summary>
		public string Center
		{
			get { return _center; }
			set { _center = value; }
		}

		private string _right = string.Empty;

		/// <summary>
		/// The text to print in the right part of the header.
		/// </summary>
		public string Right
		{
			get { return _right; }
			set { _right = value; }
		}

		private bool _line = false;

		/// <summary>
		/// Whether a line should be printed under the header (either "true" or "false").
		/// </summary>
		public bool Line
		{
			get { return _line; }
			set { _line = value; }
		}

		private float _spacing = 0.0f;

		/// <summary>
		/// The amount of space to put between the header and the content, e.g. "1.8". Be aware that if this is too large the header will be printed outside the pdf document. This can be corrected with the margin.top setting.
		/// </summary>
		public float Spacing
		{
			get { return _spacing; }
			set { _spacing = value; }
		}

		private string _htmlUrl = string.Empty;

		/// <summary>
		/// Url for a HTML document to use for the header.
		/// </summary>
		public string HtmlUrl
		{
			get { return _htmlUrl; }
			set { _htmlUrl = value; }
		}
	}
}
