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
	public class LoadSettings
	{
		public string Username { get; set; }
		public string Password { get; set; }
	    public string Proxy { get; set; }

        private string _windowStatus = "";
	    public string WindowStatus {
	        get { return _windowStatus; }
	        set { _windowStatus = value; }
	    }

	    private int _jsdelay = 200;
	    public int Jsdelay {
	        get { return _jsdelay; }
	        set { _jsdelay = value; }
	    }

	    private double _zoomFactor = 1.0;
	    public double ZoomFactor {
	        get { return _zoomFactor; }
	        set { _zoomFactor = value; }
	    }

	    private bool _stopSlowScripts = true;
	    public bool StopSlowScripts {
	        get { return _stopSlowScripts; }
	        set { _stopSlowScripts = value; }
	    }

        private LoadErrorHandlingType _loadErrorHandling = LoadErrorHandlingType.abort;
        public LoadErrorHandlingType LoadErrorHandling
        {
            get { return _loadErrorHandling; }
            set { _loadErrorHandling = value; }
        }

        // The following default to false
	    public bool RepeatCustomHeaders { get; set;}
        public bool BlockLocalFileAccess { get; set;}
        public bool DebugJavascript { get; set; }

	}
}
