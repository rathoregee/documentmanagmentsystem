using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PdfUploder.Controllers;
using PdfUploder.Data;
using PdfUploder.Models;
using PdfUploder.Services;
using PdfUploder.Validators;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PdfUploder.Tests
{
    public class DocumentControllerTests
    {
        public DocumentControllerTests() { }

        [Fact(DisplayName = "Valid pdf upload case - done ")]
        public async Task VaildPdfFileShouldStoreInDatabase()
        {
            //Given I have a PDF to upload
            var formFile = SetupFile("Sample.pdf");
            var forms = new Mock<IFormCollection>();
            forms.Setup(f => f.Files[It.IsAny<int>()]).Returns(formFile.Object);

            var client = SetupClient();
            SetSinglePutReturn(client);
            var documentFactory = new DocumentFactory();
            var iAmazonClientConnection = new Mock<IAmazonClientConnection>();
            iAmazonClientConnection.Setup(f => f.Create()).Returns(new AmazonS3Data() { Cient = client.Object });

            var _mockDataContext = new DocumentContext(new AmazonWebServicesS3(iAmazonClientConnection.Object));
            var controller = new DocumentController(_mockDataContext, documentFactory, new PdfValidator());                                 
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.Request.Form = forms.Object;

            //When I send the PDF to the API
            var response = await controller.Upload(formFile.Object);

            //Then it is uploaded successfully
            var result = response as CreatedResult;
            Assert.Equal(201, result.StatusCode);
        }

        [Fact(DisplayName = "Invalid pdf case - done ")]
        public async Task InvaildPdfFileShouldNotBeSavedInDatabase()
        {
            //Given I have a non - pdf to upload
            var _mockDataContext = new Mock<IDocumentContext>();
            var formFile = SetupFile("Sample.txt");
            var documentFactory = new Mock<IDocumentFactory>();
            documentFactory.Setup(f => f.Create(It.IsAny<Stream>())).Returns(new Document() { });
            var forms = new Mock<IFormCollection>();
            forms.Setup(f => f.Files[It.IsAny<int>()]).Returns(formFile.Object);
            var controller = new DocumentController(_mockDataContext.Object, documentFactory.Object, new PdfValidator());
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.Request.Form = forms.Object;

            //When I send the non - pdf to the API
            var response = await controller.Upload(formFile.Object);

            //Then the API does not accept the file and returns the appropriate messaging and status
            var result = response as BadRequestObjectResult;
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("please upload a valid pdf", result.Value.ToString());
        }

        [Fact(DisplayName = "Large/Max size pdf case - done ")]
        public async Task MaxSizePdfFileShouldStoreNotInDatabase()
        {
            //Given I have a max pdf size of 5MB
            var _mockDataContext = new Mock<IDocumentContext>();
            var formFile = SetupFile("5mb.pdf");
            var documentFactory = new Mock<IDocumentFactory>();
            documentFactory.Setup(f => f.Create(It.IsAny<Stream>())).Returns(new Document() { });
            var forms = new Mock<IFormCollection>();
            forms.Setup(f => f.Files[It.IsAny<int>()]).Returns(formFile.Object);
            var controller = new DocumentController(_mockDataContext.Object, documentFactory.Object, new PdfValidator());
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.Request.Form = forms.Object;

            //When I send the pdf to the API
            var response = await controller.Upload(formFile.Object);

            //Then the API does not accept the file and returns the appropriate messaging and status
            var result = response as BadRequestObjectResult;
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("File length should be greater than 0 and less than 5 MB", result.Value.ToString());
        }

        [Fact(DisplayName = "Get documents with properties - done")]
        public async Task ShouldAllDocumentsWithPropertires()
        {
            //Given I call the new document service API
            var client = SetupClient();
            SetMultipleObjectReturn(client);
            var documentFactory = new Mock<IDocumentFactory>();
            var iAmazonClientConnection = new Mock<IAmazonClientConnection>();
            iAmazonClientConnection.Setup(f => f.Create()).Returns(new AmazonS3Data() { Cient = client.Object });
            var _mockDataContext = new DocumentContext(new AmazonWebServicesS3(iAmazonClientConnection.Object));
            var controller = new DocumentController(_mockDataContext, documentFactory.Object, new PdfValidator());
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            //When I call the API to get a list of documents
            var result = await controller.Get(false);

            //Then a list of PDFs’ is returned with the following properties: name, location, file - size
            Assert.Equal(2, result.Length);
            Assert.Equal("media/abc/object1", result[0].Name);
            Assert.Equal(100, result[0].Size);
        }

        [Fact(DisplayName = "Sort order and get list of documents - done")]
        public async Task ShouldSetOrderForSubsequentCalls()  // seems integration test!!
        {
            //Given I have a list of PDFs’
            var client = SetupClient();
            SetMultipleObjectReturn(client);
            var documentFactory = new Mock<IDocumentFactory>();
            var iAmazonClientConnection = new Mock<IAmazonClientConnection>();
            iAmazonClientConnection.Setup(f => f.Create()).Returns(new AmazonS3Data() { Cient = client.Object });
            var _mockDataContext = new DocumentContext(new AmazonWebServicesS3(iAmazonClientConnection.Object));
            var controller = new DocumentController(_mockDataContext, documentFactory.Object, new PdfValidator());
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            //When I choose to re-order the list of PDFs’
            await controller.Reorder();
            var result = await controller.Get(true);

            //Then the list of PDFs’ is returned in the new order for subsequent calls to the API
            Assert.Equal(2, result.Length);
            Assert.Equal("media/abc/object2", result[0].Name);
            Assert.Equal(200, result[0].Size);
        }

        [Fact(DisplayName = "Download file case - done")]
        public async Task ShouldDownloadPdfForGivenId()
        {
            //Given I have chosen a PDF from the list API
            var client = SetupClient();
            SetSingleGetReturn(client);
            var documentFactory = new Mock<IDocumentFactory>();
            var iAmazonClientConnection = new Mock<IAmazonClientConnection>();
            iAmazonClientConnection.Setup(f => f.Create()).Returns(new AmazonS3Data() { Cient = client.Object });            
            var _mockDataContext = new DocumentContext(new AmazonWebServicesS3(iAmazonClientConnection.Object));            
            var controller = new DocumentController(_mockDataContext, documentFactory.Object, new PdfValidator());
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //When I request the location for one of the PDF's
            var result = await controller.Get(Guid.Parse("4222bf69-b536-4d19-86a8-b84aeb6643b2")) as FileStreamResult;

            //The PDF is downloaded
            Assert.NotNull(result.FileStream);
            Assert.True(result.FileDownloadName.Equals("download.pdf"));
        }

        [Fact(DisplayName = "Should not download file after delete ")]
        public async Task ShouldNoDownloadPdfForAGivenIdAfterDelete()
        {
            //Given I have selected a PDF from the list API that I no longer require
            var client = SetupClient();
            SetSingleGetReturn(client);
            SetSingleDeleteReturn(client);
            var documentFactory = new Mock<IDocumentFactory>();
            var iAmazonClientConnection = new Mock<IAmazonClientConnection>();
            iAmazonClientConnection.Setup(f => f.Create()).Returns(new AmazonS3Data() { Cient = client.Object });
            var _mockDataContext = new DocumentContext(new AmazonWebServicesS3(iAmazonClientConnection.Object));
            var controller = new DocumentController(_mockDataContext, documentFactory.Object, new PdfValidator());
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //When I request to delete the PDF
            await controller.Delete(Guid.Parse("4222bf69-b536-4d19-86a8-b84aeb6643b0"));
            var response = await controller.Get(Guid.Parse("4222bf69-b536-4d19-86a8-b84aeb6643b0"));
            var result = response as NotFoundResult;

            //Then the PDF is deleted and will no longer return from the list API and can no longer be downloaded from its location directly
            Assert.Equal(404, result.StatusCode);
        }

        [Fact(DisplayName = "Should get message for non existant - done ")]
        public async Task ShouldGetMessageForNonExistantDelete()
        {
            // Given I attempt to delete a file that does not exist
            var client = SetupClient();
            SetMultipleObjectReturn(client);
            SetSingleDeleteReturn(client);
            var documentFactory = new Mock<IDocumentFactory>();
            var iAmazonClientConnection = new Mock<IAmazonClientConnection>();
            iAmazonClientConnection.Setup(f => f.Create()).Returns(new AmazonS3Data() { Cient = client.Object });
            var _mockDataContext = new DocumentContext(new AmazonWebServicesS3(iAmazonClientConnection.Object));
            var controller = new DocumentController(_mockDataContext, documentFactory.Object, new PdfValidator());
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //When I request to delete the non-existing pdf
            var response = await controller.Delete(Guid.Parse("4222bf69-b536-4d19-86a8-b84aeb6643b0"));
            //Then the API returns an appropriate response
            var result = response as NotFoundResult;
            Assert.Equal(404, result.StatusCode);

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
