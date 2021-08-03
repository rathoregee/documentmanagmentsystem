using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PdfUploder.Services
{
    public interface IDocumentFactory
    {
        IDocument Create(Stream stream);
    }
}
