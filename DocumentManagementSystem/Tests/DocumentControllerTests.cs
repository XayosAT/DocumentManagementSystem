using Moq;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Xunit;
using FluentValidation;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DAL.Services;
using REST.Controllers;
using SharedData.DTOs;
using SharedData.EntitiesDAL;
using SharedData.EntitiesBL;
using AutoMapper;
using DAL.Repositories;
using DAL.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Minio.DataModel.Response;

public class DocumentControllerTests
{
    private readonly Mock<IMinioClient> _minioClientMock;
    private readonly Mock<IDocumentRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<DocumentDAL>> _dalValidatorMock;
    private readonly Mock<IValidator<DocumentBL>> _blValidatorMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly DocumentController _controller;

    public DocumentControllerTests()
    {
        // Create mocks for all dependencies
        _minioClientMock = new Mock<IMinioClient>();
        _repositoryMock = new Mock<IDocumentRepository>();
        _mapperMock = new Mock<IMapper>();
        _dalValidatorMock = new Mock<IValidator<DocumentDAL>>();
        _blValidatorMock = new Mock<IValidator<DocumentBL>>();
        _publisherMock = new Mock<IMessagePublisher>();

        // Mock IConfiguration if necessary
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.SetupGet(c => c["Minio:BucketName"]).Returns("uploads");

        // Create DocumentService with all mocks
        var documentService = new DocumentService(
            _repositoryMock.Object,
            _mapperMock.Object,
            _dalValidatorMock.Object,
            _blValidatorMock.Object,
            _publisherMock.Object,
            _minioClientMock.Object,
            configurationMock.Object
        );

        // Create controller
        _controller = new DocumentController(documentService);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnAllDocuments()
    {
        // Arrange
        var documents = new List<DocumentDTO>
        {
            new DocumentDTO { Id = 1, Name = "Doc1", Path = "Path1", FileType = ".txt" },
            new DocumentDTO { Id = 2, Name = "Doc2", Path = "Path2", FileType = ".pdf" }
        };
        
        _repositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<DocumentDAL> { new DocumentDAL { Id = 1 }, new DocumentDAL { Id = 2 } });

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<DocumentDTO>>(It.IsAny<IEnumerable<DocumentDAL>>()))
            .Returns(documents);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDocuments = Assert.IsAssignableFrom<IEnumerable<DocumentDTO>>(okResult.Value);
        Assert.Equal(2, ((List<DocumentDTO>)returnedDocuments).Count);
    }

    // [Fact]
    // public async Task Post_ShouldReturnOk_WhenFileIsValid()
    // {
    //     // Arrange
    //     var fileMock = new Mock<IFormFile>();
    //     var content = "Test file content";
    //     var fileName = "test.txt";
    //     var ms = new MemoryStream();
    //     var writer = new StreamWriter(ms);
    //     writer.Write(content);
    //     writer.Flush();
    //     ms.Position = 0;
    //
    //     fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
    //     fileMock.Setup(f => f.FileName).Returns(fileName);
    //     fileMock.Setup(f => f.Length).Returns(ms.Length);
    //     fileMock.Setup(f => f.ContentType).Returns("text/plain");
    //
    //     // Prepare a valid instance of PutObjectResponse
    //     var putObjectResponse = new PutObjectResponse("requestId", "bucketName", fileName, "etag", "versionId");
    //
    //     // Mock successful upload to MinIO to return a valid PutObjectResponse
    //     _minioClientMock.Setup(minio => minio.PutObjectAsync(It.IsAny<PutObjectArgs>(), default))
    //         .ReturnsAsync(putObjectResponse);
    //
    //     // Mock Repository to return a completed task on AddAsync
    //     _repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<DocumentDAL>())).Returns(Task.CompletedTask);
    //
    //     // Mock DAL Validator to not produce any validation errors
    //     _dalValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<DocumentDAL>(), default))
    //         .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    //
    //     // Act
    //     var result = await _controller.Post(fileMock.Object);
    //
    //     // Assert
    //     var okResult = Assert.IsType<OkObjectResult>(result);
    //
    //     // Assert that the response is a dictionary containing the keys "message" and "documentId"
    //     var response = okResult.Value as IDictionary<string, object>;
    //     Assert.NotNull(response);
    //     Assert.Equal("Document successfully uploaded.", response["message"]);
    //     Assert.NotNull(response["documentId"]);
    // }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenDocumentIsDeleted()
    {
        // Arrange
        int documentId = 1;

        _repositoryMock.Setup(repo => repo.GetByIdAsync(documentId))
            .ReturnsAsync(new DocumentDAL { Id = documentId });

        _repositoryMock.Setup(repo => repo.DeleteAsync(documentId)).Returns(Task.CompletedTask);
        
        // Act
        var result = await _controller.DeleteAsync(documentId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        int documentId = 1;

        _repositoryMock.Setup(repo => repo.GetByIdAsync(documentId))
            .ReturnsAsync((DocumentDAL)null);

        // Act
        var result = await _controller.DeleteAsync(documentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        int documentId = 1;
        var documentDTO = new DocumentDTO { Id = documentId, Name = "UpdatedName", Path = "UpdatedPath", FileType = ".txt" };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(documentId))
            .ReturnsAsync(new DocumentDAL { Id = documentId });

        _blValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<DocumentBL>(), default))
                        .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _repositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<DocumentDAL>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateAsync(documentId, documentDTO);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        int documentId = 1;
        var documentDTO = new DocumentDTO { Id = documentId, Name = "UpdatedName", Path = "UpdatedPath", FileType = ".txt" };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(documentId))
            .ReturnsAsync((DocumentDAL)null);

        // Act
        var result = await _controller.UpdateAsync(documentId, documentDTO);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        int documentId = 1;
        var documentDTO = new DocumentDTO { Id = documentId, Name = "UpdatedName", Path = "UpdatedPath", FileType = ".txt" };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(documentId))
            .ReturnsAsync(new DocumentDAL { Id = documentId });

        _blValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<DocumentBL>(), default))
                        .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
                        {
                            new FluentValidation.Results.ValidationFailure("Name", "Invalid Name")
                        }));

        // Act
        var result = await _controller.UpdateAsync(documentId, documentDTO);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
