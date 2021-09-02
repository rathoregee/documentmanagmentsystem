using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PdfUploder.Controllers;
using PdfUploder.Data;
using PdfUploder.Models;
using PdfUploder.Services;
using PdfUploder.Tests.UnitTests;
using PdfUploder.Validators;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PdfUploder.Autofixture.UnitTests.Tests
{
    public class DocumentController_AutofixtureTests
    {
        [Theory(DisplayName = "Sample for autofixture testing "), AutoMoqData]
        public async Task SampleTest([Greedy] DocumentController sut)
        {
            //Given I have a PDF to upload
            var formFile = SetupFile("Sample.pdf");
            sut.ControllerContext.HttpContext = new DefaultHttpContext();

            //When I send the PDF to the API
            var response = await sut.Upload(formFile.Object);

            //Then it is uploaded successfully
            var result = response as CreatedResult;
            Assert.Equal(201, result.StatusCode);
        }

        // TODO: Rahul please refactor this one
        [Theory(DisplayName = "Valid pdf upload case"), AutoMoqData]
        public async Task SampleTest2(
            [Frozen] Mock<IAmazonClientConnection> iAmazonClientConnection,
            [Frozen] Mock<AmazonS3Client> s3,         
            [Frozen] DocumentFactory documentFactory,
            [Frozen] CustomFileFactory content,
            [Frozen] PdfValidator validator
            )
        {
            //Given I have a PDF to upload
            var formFile = SetupFile("Sample.pdf");
            SetSinglePutReturn(s3);
            iAmazonClientConnection.Setup(f => f.Create()).Returns(new AmazonS3Data() { Cient = s3.Object });
            var _mockDataContext = new DocumentContext(new AmazonWebServicesS3(iAmazonClientConnection.Object));
            var sut = new DocumentController(_mockDataContext, documentFactory, content, validator);
            sut.ControllerContext.HttpContext = new DefaultHttpContext();
            //When I send the PDF to the API
            var response = await sut.Upload(formFile.Object);

            //Then it is uploaded successfully
            var result = response as CreatedResult;
            Assert.Equal(201, result.StatusCode);
        } 

        #region private methods
        private Mock<IFormFile> SetupFile(string fileName)
        {
            var file = new Mock<IFormFile>();
            var source = File.ReadAllBytes(fileName);
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(source);
            writer.Flush();
            stream.Position = 0;
            file.Setup(f => f.OpenReadStream()).Returns(stream);
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.Length).Returns(source.Length);

            return file;
        }

        private Mock<AmazonS3Client> SetupClient()
        {
            return new Mock<AmazonS3Client>(FallbackCredentialsFactory.GetCredentials(true), new AmazonS3Config { RegionEndpoint = RegionEndpoint.EUWest2 });
        }

        private Stream SetSingleGetReturn(Mock<AmazonS3Client> s3ClientMock, string pathToTestDoc = "sample.pdf")
        {
            var docStream = new FileInfo(pathToTestDoc).OpenRead();
            s3ClientMock
              .Setup(x => x.GetObjectAsync(
                 It.IsAny<string>(),
                 It.IsAny<string>(),
                 It.IsAny<CancellationToken>()))
              .ReturnsAsync(
                 GetObjectResponsetSetup(docStream));

            return docStream;
        }

        private void SetSinglePutReturn(Mock<AmazonS3Client> s3ClientMock)
        {          
            s3ClientMock
              .Setup(x => x.PutObjectAsync(
                 It.IsAny<PutObjectRequest>(),
                 It.IsAny<CancellationToken>()))
              .ReturnsAsync(
                 PutObjectResponseSetup());            
        }

        private void SetMultipleObjectReturn(Mock<AmazonS3Client> s3ClientMock)
        {
            var response = new ListObjectsResponse { IsTruncated = false };
            response.S3Objects.AddRange(new[] {
                new S3Object { Key = "media/abc/object1", Size=100 },
                new S3Object { Key = "media/abc/object2", Size=200 }
            });

            s3ClientMock.Setup(x => x.ListObjectsAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
                )).ReturnsAsync(response);
        }

        private static Func<PutObjectRequest, CancellationToken, PutObjectResponse> PutObjectResponseSetup()
        {
            return (PutObjectRequest request, CancellationToken ct) => PutObjectResponse();
        }

        private static PutObjectResponse PutObjectResponse()
        {
            return new PutObjectResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };
        }

        private static Func<string, string, CancellationToken, GetObjectResponse> GetObjectResponsetSetup(FileStream docStream)
        {
            return (string bucket, string key, CancellationToken ct) =>
                               GetObjectResponse(bucket, key, docStream);
        }
        private static GetObjectResponse GetObjectResponse(string bucket, string key, FileStream docStream)
        {
            return new GetObjectResponse
            {
                BucketName = bucket,
                Key = key,
                HttpStatusCode = HttpStatusCode.OK,
                ResponseStream = docStream,
            };
        }


        private void SetSingleDeleteReturn(Mock<AmazonS3Client> s3ClientMock)
        {
            s3ClientMock
              .Setup(x => x.DeleteObjectAsync(
                 It.IsAny<DeleteObjectRequest>(),
                 It.IsAny<CancellationToken>()))
              .ReturnsAsync(
                 DeleteObjectResponse());
        }

        private static DeleteObjectResponse DeleteObjectResponse()
        {
            return new DeleteObjectResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };
        }

        #endregion private methods
    }
}
