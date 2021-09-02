using PdfUploder.Models;
using System.IO;

namespace PdfUploder.Services
{
    public class CustomFileFactory : ICustomFileFactory
    {
        public CustomFileContent Create(IDocument document)
        {
            return new CustomFileContent
            {
                Stream = new MemoryStream(document.File),
                FileName = "download.pdf",
                Type = "APPLICATION/octet-stream"
            };
        }
    }
}
