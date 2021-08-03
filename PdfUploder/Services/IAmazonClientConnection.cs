using PdfUploder.Models;

namespace PdfUploder.Services
{
    public interface IAmazonClientConnection
    {
        AmazonS3Data Create();
    }
}
