using Amazon;
using Amazon.S3;
using PdfUploder.Models;
using System;

namespace PdfUploder.Services
{
    public class AmazonClientConnection : IAmazonClientConnection
    {
        public AmazonS3Data Create()
        {
            //var client =  System.Diagnostics.Debugger.IsAttached ? new AmazonS3Client("AccessKeyId", "SecretKeyId", RegionEndpoint.EUWest2) :
            var client = new AmazonS3Client(Environment.GetEnvironmentVariable("S3AccessKeyId"), Environment.GetEnvironmentVariable("S3SecretAccessKeyId"), RegionEndpoint.EUWest2);
            return new AmazonS3Data() { Cient = client };
        }
    }
}
