using System;

namespace WkHtmlToXSharp
{
    public interface IHtmlToXConverter : IDisposable
    {
        event EventHandler<EventArgs<int>> Begin;
        event EventHandler<EventArgs<string>> Error;
        event EventHandler<EventArgs<bool>> Finished;
        event EventHandler<EventArgs<int, string>> PhaseChanged;
        event EventHandler<EventArgs<int, string>> ProgressChanged;
        event EventHandler<EventArgs<string>> Warning;

        IGlobalSettings GlobalSettings { get; }
        IObjectSettings ObjectSettings { get; }

        byte[] Convert();
        byte[] Convert(string inputHtml);
    }

    /// <summary>
    /// Generic PDF conversion interface.
    /// </summary>
    public interface IHtmlToPdfConverter : IHtmlToXConverter
    {
        PdfGlobalSettings PdfGlobalSettings { get; }
        PdfObjectSettings PdfObjectSettings { get; }
    }

    /// <summary>
    /// Generic image conversion interface.
    /// </summary>
    public interface IHtmlToImageConverter : IHtmlToXConverter
    {
        ImageGlobalSettings ImageGlobalSettings { get; }
    }

    public interface IGlobalSettings
    {
        int Dpi { get; }
        string Out { get; }
    }

    public interface IObjectSettings
    {
        string Page { get; }
    }
}
