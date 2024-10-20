using Xunit;
using Moq;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using DocumentManagementSystem.Controllers;
using DocumentManagementSystem.DTOs;
using System.IO;
using System.Net.Http.Json;
using Moq.Protected;
using System.Threading;
using Microsoft.AspNetCore.Http;

public class DocumentControllerTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly DocumentController _controller;

    public DocumentControllerTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var client = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new System.Uri("https://localhost:8081") // Set the BaseAddress to a valid URI
        };

        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        _controller = new DocumentController(_httpClientFactoryMock.Object);
    }

    // Helper method to setup HttpClient responses
    private void SetupHttpResponse(HttpStatusCode statusCode, object content)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(JsonSerializer.Serialize(content))
            });
    }

    [Fact]
    public async Task Get_ShouldReturnAllDocuments_WhenResponseIsSuccessful()
    {
        // Arrange
        var documentList = new List<DocumentDTO>
        {
            new DocumentDTO { Name = "Document1", Path = "Path1", FileType = "pdf" },
            new DocumentDTO { Name = "Document2", Path = "Path2", FileType = "docx" }
        };

        SetupHttpResponse(HttpStatusCode.OK, documentList);

        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDocuments = Assert.IsType<List<DocumentDTO>>(okResult.Value);
        Assert.Equal(2, returnedDocuments.Count);
    }

    [Fact]
    public async Task Get_ShouldReturnStatusCode_WhenRequestFails()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Failed to retrieve documents");

        // Act
        var result = await _controller.Get();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Post_ShouldUploadFileAndReturnOk_WhenFileIsValid()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var content = "Test file content";
        var fileName = "test.txt";

        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);

        SetupHttpResponse(HttpStatusCode.OK, new { });

        // Act
        var result = await _controller.Post(fileMock.Object);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Post_ShouldReturnBadRequest_WhenFileIsInvalid()
    {
        // Arrange
        IFormFile file = null;

        // Act
        var result = await _controller.Post(file);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is null or empty", badRequestResult.Value);
    }
}