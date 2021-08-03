using PdfUploder.Services;
using System;
using System.Threading.Tasks;

namespace PdfUploder.Data
{
    public class DocumentContext : IDocumentContext
    {
        public IAmazonWebServicesS3 _context { get; }

        public DocumentContext(IAmazonWebServicesS3 context)
        {
            _context = context;
        }       

        public async Task<bool> CreateAsync(IDocument document)
        {
            var response = await _context.WriteAsync("Folder", document.Name, (object)document.File);
            return (int)response.HttpStatusCode == 200;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var data = await _context.ReadAsync("Folder", id.ToString());

            if (data == null)
                return false;            

            var response = await _context.DeleteAsync("Folder", id.ToString());

            return (int)response.HttpStatusCode == 200;
        }

        public async Task<Document[]> GetAllAsync(bool useOrder)
        {
            return await _context.ReadListAsync("Folder", useOrder ? ListSorter.IsDecending : false);
        }

        public async Task<Document> GetAsync(Guid id)
        {
            var data = await _context.ReadAsync("Folder", id.ToString());
            return await Task.FromResult(new Document { File = data });
        }
        public async Task<bool> ChangeSortOrder()
        {
            ListSorter.IsDecending = !ListSorter.IsDecending;

            return await Task.FromResult(true);
        }
    }

    public static class ListSorter
    {
        public static bool IsDecending { get; set; }
    }
}
