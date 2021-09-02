using System;
using System.IO;

namespace PdfUploder
{
    public interface IDocument
    {
        Guid Id { get; set; }
        string Location { get; set; }
        byte[] File { get; set; }
        long Size { get; set; }
        string Name { get; set; }
    }
}