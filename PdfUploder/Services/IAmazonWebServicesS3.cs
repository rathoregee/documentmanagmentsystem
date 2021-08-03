using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace PdfUploder.Services
{
    public interface IAmazonWebServicesS3
    {
        Task<DeleteObjectResponse> DeleteAsync(string srcBucket, string srcKey);        
        string ReadBase64String(Stream stream);
        Task<byte[]> ReadAsync(string bucketName, string keyName);
        Task<PutObjectResponse> WriteAsync(string bucketName, string keyName, object fileData);
        Task<Document[]> ReadListAsync(string bucketName, bool isDecending);
    }
}