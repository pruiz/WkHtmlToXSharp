using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WkHtmlToXSharp
{
	/// <summary>
	/// Bunclde containing native libraries for an specific platform.
	/// </summary>
	public interface INativeLibraryBundle
	{
		/// <summary>
		/// Gets a value indicating whether this instance can support current platform.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance can support current platform; otherwise, <c>false</c>.
		/// </value>
		bool SupportsCurrentPlatform { get; }
		/// <summary>
		/// Deploys the libraries bundle into the currently executing environment.
		/// </summary>
		void DeployBundle(WkHtmlToXLibrariesManager manager);
	}
}
