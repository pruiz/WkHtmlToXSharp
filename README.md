This is a C# wrapper (using P/Invoke) for the excelent Html to PDF conversion library wkhtmltopdf.

CURRENT STATUS & LIMITATIONS
----------------------------

I've been using it on production since Q4/2010, and has worked fine so far, so we might say it's almost 'production ready'..

The way I'm using the component is from a back office windows service which handles html->pdf conversion as part of some workflows initiated by users from a web application, so, the web application does not directly call WkHtmlToXSharp, but instead sends a message thru NServiceBus to an independent service which handles (among other tasks) generation of a PDF
from Html, a Pdf document which it's later made available to our user thru the web application..

As I said, it is working quite nicely, however, I think I should advise you about the current limitations of the API:

 * **Concurrent conversions**: The component exposes two main conversion classes 'WkHtmlToPdfConverter' & 'MultiplexingConverter', both serving as a mean to convert from html to pdf, WkHtmlToPdfConverter it's a plain OO wrapper against wkhtmltox.dll's API, but, due to an wkhtmltox.dll's limitation, all calls to it's API should be made from the same thread, which can be a problem on a web application. To somewhat avoid such a limitation I have defined a class named 'MultiplexingConverter' which uses and internally created thread to perform conversions serializing calls to the underlaying C library within such thread.

  The API it's exactly the same one as WkHtmlToPdfConverter exposes, it is just a wrapper which handles (and hides) complexities involved in using the same thread for all calls. Also, this imposes a performance hit (as conversions are not really run concurrently, but serialized under a single 'proxy' thread), so if your application is in the need of massive concurrent conversions, this might be a problem.

 * **No html to image conversions yet**: Currently the API only supports Html->Pdf conversions, although wkhtmltox.dll supports html->image too.. While implementing WkHtmlToImage should be pretty easy, it's still on TODO due to time constraints.

 * At the moment of writting site http://wkhtmltopdf.org/downloads.html provides both x64 and x32 bits version of wkhtmltopdf application installs. I took the wkhtmltox.dll library of x64 application which can be found under bin folder in placed where application was installed and used in x64 wrapper project. In order to keep compatibility with the rest of the projects I renamed wkhtmltox.dll library to wkhtmltox0.dll and gizziped it. The resulting file was added the the project as embedded resource in order to keep deployment mechanism consistent. It seems also that wkhtmltox.dll library comes with all other dependencies embedded so wrapper doesn't require other dependencies like libeay32.dll, ssleay32.dll, etc to be present. I didn't try to replace the same wrapper for x32 version of wrapper.

 * **Normal P/Invoke restrictions**: This assembly uses P/Invoke to call wkhtmltox.dll, so normal restrictions deriving from using P/Invoke apply..

As a final note, I should note that when using this component there's no need to download nor install wkhtmltox.dll, as native .dll/.so files needed are embedded into WkHtmlToXSharp.dll and they are deployed automatically the first time a conversion is performed.

This makes the dll bigger (even if native dll's are embedded as gzipped resources), but this makes things much more fun & easier.. ;)


