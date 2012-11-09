using System;
namespace WkHtmlToXSharp
{
    /// <summary>
    /// Generic image conversion interface.
    /// </summary>
    public interface IHtmlToImageConverter : IDisposable
    {
        event EventHandler<EventArgs<int>> Begin;
        event EventHandler<EventArgs<string>> Error;
        event EventHandler<EventArgs<bool>> Finished;
        event EventHandler<EventArgs<int, string>> PhaseChanged;
        event EventHandler<EventArgs<int, string>> ProgressChanged;
        event EventHandler<EventArgs<string>> Warning;

        ImageGlobalSettings GlobalSettings { get; }

        byte[] Convert();
        byte[] Convert(string inputHtml);
    }
}
