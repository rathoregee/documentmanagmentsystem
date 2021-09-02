using System;
using System.IO;

namespace PdfUploder
{
    public class Document : IDocument
    {
        public Guid Id { get; set; }
        public string Location { get; set; }
        public byte[] File { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }
    }
}
