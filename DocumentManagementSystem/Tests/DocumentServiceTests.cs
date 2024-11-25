using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using DAL.RabbitMQ;
using DAL.Repositories;
using DAL.Services;
using FluentValidation;
using FluentValidation.Results;
using log4net;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel.Args;
using Moq;
using SharedData.DTOs;
using SharedData.EntitiesBL;
using SharedData.EntitiesDAL;
using Xunit;
using Microsoft.Extensions.Configuration;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<DocumentDAL>> _dalValidatorMock;
    private readonly Mock<IValidator<DocumentBL>> _blValidatorMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly Mock<IMinioClient> _minioClientMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        // Setup mocks
        _repositoryMock = new Mock<IDocumentRepository>();
        _mapperMock = new Mock<IMapper>();
        _dalValidatorMock = new Mock<IValidator<DocumentDAL>>();
        _blValidatorMock = new Mock<IValidator<DocumentBL>>();
        _publisherMock = new Mock<IMessagePublisher>();
        _minioClientMock = new Mock<IMinioClient>();
        _configurationMock = new Mock<IConfiguration>();

        // Set a mock configuration value for MinIO bucket name
        _configurationMock.SetupGet(c => c["Minio:BucketName"]).Returns("uploads");

        // Instantiate the DocumentService with the mocks
        _documentService = new DocumentService(
            _repositoryMock.Object,
            _mapperMock.Object,
            _dalValidatorMock.Object,
            _blValidatorMock.Object,
            _publisherMock.Object,
            _minioClientMock.Object,
            _configurationMock.Object
        );
    }

    [Fact]
    public async Task GetAllDocumentsAsync_ShouldReturnMappedDocuments()
    {
        // Arrange
        var documentsDAL = new List<DocumentDAL> { new DocumentDAL { Id = 1 }, new DocumentDAL { Id = 2 } };
        _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(documentsDAL);

        var documentsDTO = new List<DocumentDTO> { new DocumentDTO { Id = 1 }, new DocumentDTO { Id = 2 } };
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<DocumentDTO>>(documentsDAL)).Returns(documentsDTO);

        // Act
        var result = await _documentService.GetAllDocumentsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _repositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ShouldReturnDocument_WhenDocumentExists()
    {
        // Arrange
        var documentDAL = new DocumentDAL { Id = 1 };
        var documentDTO = new DocumentDTO { Id = 1 };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(documentDAL);
        _mapperMock.Setup(mapper => mapper.Map<DocumentDTO>(documentDAL)).Returns(documentDTO);

        // Act
        var result = await _documentService.GetDocumentByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    // [Fact]
    // public async Task UploadDocumentAsync_ShouldUploadFile_WhenFileIsValid()
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
    //     // Mock PutObjectAsync for MinIO
    //     _minioClientMock.Setup(minio => minio.PutObjectAsync(It.IsAny<PutObjectArgs>(), default))
    //                     .ReturnsAsync(Mock.Of<Minio.DataModel.Response.PutObjectResponse>());
    //
    //     // Mock mapping and validation
    //     var documentDTO = new DocumentDTO { Name = fileName, Path = $"minio://uploads/{fileName}", FileType = ".txt" };
    //     var documentDAL = new DocumentDAL { Name = fileName, Path = $"minio://uploads/{fileName}", FileType = ".txt" };
    //
    //     _mapperMock.Setup(mapper => mapper.Map<DocumentDAL>(documentDTO)).Returns(documentDAL);
    //     _dalValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<DocumentDAL>(), default))
    //                      .ReturnsAsync(new ValidationResult());
    //
    //     // Act
    //     var result = await _documentService.UploadDocumentAsync(fileMock.Object);
    //
    //     // Assert
    //     Assert.NotNull(result);
    //     _minioClientMock.Verify(minio => minio.PutObjectAsync(It.IsAny<PutObjectArgs>(), default), Times.Once);
    //     _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<DocumentDAL>()), Times.Once);
    // }

    [Fact]
    public async Task UpdateDocumentAsync_ShouldUpdateDocument_WhenValidationPasses()
    {
        // Arrange
        var documentId = 1;
        var documentDTO = new DocumentDTO { Id = documentId, Name = "UpdatedName", Path = "UpdatedPath", FileType = ".txt" };
        var documentDAL = new DocumentDAL { Id = documentId };
        var documentBL = new DocumentBL { Id = documentId };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(documentId)).ReturnsAsync(documentDAL);
        _mapperMock.Setup(mapper => mapper.Map<DocumentBL>(documentDTO)).Returns(documentBL);
        _blValidatorMock.Setup(v => v.ValidateAsync(documentBL, default))
                        .ReturnsAsync(new ValidationResult());

        // Act
        await _documentService.UpdateDocumentAsync(documentId, documentDTO);

        // Assert
        _repositoryMock.Verify(repo => repo.UpdateAsync(documentDAL), Times.Once);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldDeleteDocument_WhenDocumentExists()
    {
        // Arrange
        var documentId = 1;
        var documentDAL = new DocumentDAL { Id = documentId, Name = "test.txt" };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(documentId)).ReturnsAsync(documentDAL);
        _minioClientMock.Setup(minio => minio.RemoveObjectAsync(It.IsAny<RemoveObjectArgs>(), default))
                        .Returns(Task.CompletedTask);

        // Act
        await _documentService.DeleteDocumentAsync(documentId);

        // Assert
        _minioClientMock.Verify(minio => minio.RemoveObjectAsync(It.IsAny<RemoveObjectArgs>(), default), Times.Once);
        _repositoryMock.Verify(repo => repo.DeleteAsync(documentId), Times.Once);
    }

    // [Fact]
    // public async Task GetFileAsync_ShouldReturnFile_WhenFileExists()
    // {
    //     // Arrange
    //     var fileName = "test.txt";
    //     var ms = new MemoryStream();
    //     var content = "This is a test file.";
    //     var writer = new StreamWriter(ms);
    //     writer.Write(content);
    //     writer.Flush();
    //     ms.Position = 0;
    //
    //     _minioClientMock.Setup(minio => minio.GetObjectAsync(It.IsAny<GetObjectArgs>(), default))
    //                     .Callback<GetObjectArgs, CancellationToken>((args, token) =>
    //                     {
    //                         args.CallbackStream(ms);
    //                     })
    //                     .Returns(Task.CompletedTask);
    //
    //     // Act
    //     var result = await _documentService.GetFileAsync(fileName);
    //
    //     // Assert
    //     Assert.NotNull(result);
    //     Assert.Equal(ms.ToArray(), result);
    // }
}
