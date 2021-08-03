using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using PdfUploder.Models;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
namespace PdfUploder.Services
{
    public class AmazonWebServicesS3 : IAmazonWebServicesS3
    {
        private IAmazonClientConnection _context { get; }
        private AmazonS3Data _config;

        public AmazonWebServicesS3(IAmazonClientConnection client)
        {
            _context = client;
        }
        private AmazonS3Client GetClient()
        {
            _config = _context.Create();
            return _config.Cient;
        }

        public async Task<PutObjectResponse> WriteAsync(string bucketName, string keyName, object fileData)
        {
            var amazonS3Client = GetClient();
            var jsonData = JsonConvert.SerializeObject(fileData);
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                ContentType = "application/json",
                Key = keyName,
                ContentBody = jsonData
            };

            var putObjectResponse = await amazonS3Client.PutObjectAsync(putObjectRequest);
            return putObjectResponse;
        }


        public async Task<byte[]> ReadAsync(string bucketName, string keyName)
        {
            try
            {
                var S3Client = GetClient();
                using (GetObjectResponse response = await S3Client.GetObjectAsync(bucketName, keyName, new CancellationToken() { }))
                using (Stream responseStream = response.ResponseStream)
                using (var memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<Document[]> ReadListAsync(string bucketName, bool isDecending)
        {
            var S3Client = GetClient();
            var list = new List<Document>();
            var response = await S3Client.ListObjectsAsync(bucketName, new CancellationToken() { });

            foreach (S3Object x in response.S3Objects)
            {
                list.Add(new Document { Name = x.Key, Size = x.Size });
            }

            if (isDecending)
                list.Reverse();

            return list.ToArray();
        }

        public async Task<DeleteObjectResponse> DeleteAsync(string srcBucket, string srcKey)
        {
            var amazonS3Client = GetClient();
            var request = new DeleteObjectRequest
            {
                BucketName = srcBucket,
                Key = srcKey
            };

            var response = await amazonS3Client.DeleteObjectAsync(request);
            return response;
        }

        public string ReadBase64String(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}