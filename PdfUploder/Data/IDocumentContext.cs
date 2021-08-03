using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfUploder.Data
{
    public interface IDocumentContext
    {
        Task<Document[]> GetAllAsync(bool useOrder);
        Task<bool> CreateAsync(IDocument document);
        Task<Document> GetAsync(Guid id);        
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ChangeSortOrder();
    }
}