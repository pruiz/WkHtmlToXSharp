using System;
namespace WkHtmlToXSharp
{
	/// <summary>
	/// Generic PDF conversion interface.
	/// </summary>
	public interface IHtmlToPdfConverter : IDisposable
	{
		event EventHandler<EventArgs<int>> Begin;
		event EventHandler<EventArgs<string>> Error;
		event EventHandler<EventArgs<bool>> Finished;
		event EventHandler<EventArgs<int, string>> PhaseChanged;
		event EventHandler<EventArgs<int, string>> ProgressChanged;
		event EventHandler<EventArgs<string>> Warning;

		PdfGlobalSettings GlobalSettings { get; }
		PdfObjectSettings ObjectSettings { get; }

		byte[] Convert();
		byte[] Convert(string inputHtml);
	}
}
