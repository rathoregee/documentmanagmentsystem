using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PdfUploder.Models
{
    public class CustomFileContent
    {
        public MemoryStream Stream { get; set; }
        public string Type { get; set; }
        public string FileName { get; set; }
    }
}
