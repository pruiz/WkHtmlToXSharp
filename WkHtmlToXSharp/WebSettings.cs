#region Copyright
//
// Author: Pablo Ruiz García (pruiz@crt0.net)
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
	public class WebSettings
	{
	    private bool _background = true;
	    public bool Background {
	        get { return _background; }
	        set { _background = value; }
	    }

	    private bool _loadImages = true;
	    public bool LoadImages {
	        get { return _loadImages; }
	        set { _loadImages = value; }
	    }

	    private bool _enablePlugins = false;
	    public bool EnablePlugins {
	        get { return _enablePlugins; }
	        set { _enablePlugins = value; }
	    }

	    private bool _enableJavascript = true;
	    public bool EnableJavascript {
	        get { return _enableJavascript; }
	        set { _enableJavascript = value; }
	    }

	    private string _defaultEncoding = "";
	    public string DefaultEncoding {
	        get { return _defaultEncoding; }
	        set { _defaultEncoding = value; }
	    }

        private bool _enableIntelligentShrinking = true;
	    public bool EnableIntelligentShrinking {
	        get { return _enableIntelligentShrinking; }
	        set { _enableIntelligentShrinking = value; }
	    }

        private int _minimumFontSize = -1;
	    public int MinimumFontSize {
	        get { return _minimumFontSize; }
	        set { _minimumFontSize = value; }
	    }

        private bool _printMediaType = false;
	    public bool PrintMediaType {
	        get { return _printMediaType; }
	        set { _printMediaType = value; }
	    }

        private string _userStyleSheet = "";
	    public string UserStyleSheet {
	        get { return _userStyleSheet; }
	        set { _userStyleSheet = value; }
	    }

	}
}
