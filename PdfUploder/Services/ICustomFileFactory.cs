using PdfUploder.Models;
namespace PdfUploder.Services
{
    public interface ICustomFileFactory
    {
        CustomFileContent Create(IDocument document);
    }
}
