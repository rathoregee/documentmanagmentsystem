using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PdfUploder.Services
{
    public class DocumentFactory : IDocumentFactory
    {
        public IDocument Create(Stream stream)
        {
            var ms = (MemoryStream)stream;
            byte[] bytes = ms.ToArray();
            return new Document { Id = new Guid(), File = bytes };
        }
    }
}
