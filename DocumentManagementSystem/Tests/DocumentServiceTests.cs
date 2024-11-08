using Moq;
using Xunit;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using DAL.Repositories;
using DAL.Services;
using AutoMapper;
using SharedData.DTOs;
using SharedData.EntitiesDAL;
using DAL.RabbitMQ;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using SharedData.EntitiesBL;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<DocumentDAL>> _dalValidatorMock;
    private readonly Mock<IValidator<DocumentBL>> _blValidatorMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _repositoryMock = new Mock<IDocumentRepository>();
        _mapperMock = new Mock<IMapper>();
        _dalValidatorMock = new Mock<IValidator<DocumentDAL>>();
        _blValidatorMock = new Mock<IValidator<DocumentBL>>();
        _publisherMock = new Mock<IMessagePublisher>();

        _documentService = new DocumentService(
            _repositoryMock.Object,
            _mapperMock.Object,
            _dalValidatorMock.Object,
            _blValidatorMock.Object,
            _publisherMock.Object
        );
    }

    [Fact]
    public async Task GetAllDocumentsAsync_ShouldReturnDocuments_WhenDocumentsExist()
    {
        // Arrange
        var documentsDAL = new List<DocumentDAL>
        {
            new DocumentDAL { Id = 1, Name = "Document1", Path = "Path1", FileType = "pdf" },
            new DocumentDAL { Id = 2, Name = "Document2", Path = "Path2", FileType = "docx" }
        };

        _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(documentsDAL);
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<DocumentDTO>>(documentsDAL))
            .Returns(documentsDAL.Select(doc => new DocumentDTO
                { Name = doc.Name, Path = doc.Path, FileType = doc.FileType }));

        // Act
        var result = await _documentService.GetAllDocumentsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ShouldReturnDocument_WhenDocumentExists()
    {
        // Arrange
        var documentDAL = new DocumentDAL { Id = 1, Name = "Document1", Path = "Path1", FileType = "pdf" };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(documentDAL);
        _mapperMock.Setup(mapper => mapper.Map<DocumentDTO>(documentDAL))
            .Returns(new DocumentDTO
                { Name = documentDAL.Name, Path = documentDAL.Path, FileType = documentDAL.FileType });

        // Act
        var result = await _documentService.GetDocumentByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Document1", result.Name);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((DocumentDAL)null);

        // Act
        var result = await _documentService.GetDocumentByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetFile_ShouldReturnFileBytes_WhenFileExists()
    {
        // Arrange
        var filename = "document.pdf";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", filename);
        var content = new byte[] { 1, 2, 3, 4 };
        File.WriteAllBytes(filePath, content);

        string contentType;
        var result = _documentService.GetFile(filename, out contentType);

        // Assert
        Assert.Equal(content, result);
        Assert.Equal("application/pdf", contentType);

        // Clean up
        File.Delete(filePath);
    }

    [Fact]
    public void GetFile_ShouldReturnNull_WhenFileDoesNotExist()
    {
        // Arrange
        string contentType;
        var result = _documentService.GetFile("nonexistent.pdf", out contentType);

        // Assert
        Assert.Null(result);
        Assert.Null(contentType);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldUploadFile_WhenFileIsValid()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var fileName = "test.pdf";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);
        var fileStream = new MemoryStream();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(fileStream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);

        var documentDAL = new DocumentDAL { Id = 1, Name = fileName, Path = filePath, FileType = ".pdf" };

        _mapperMock.Setup(mapper => mapper.Map<DocumentDAL>(It.IsAny<DocumentDTO>())).Returns(documentDAL);
        _dalValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<DocumentDAL>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<DocumentDAL>())).Returns(Task.CompletedTask);

        // Act
        var result = await _documentService.UploadDocumentAsync(fileMock.Object);

        // Assert
        Assert.Equal("1", result); // Document ID returned as string
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldThrowValidationException_WhenFileIsInvalid()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var fileName = "invalid.pdf";
        var fileStream = new MemoryStream();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(fileStream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);

        var documentDAL = new DocumentDAL { Id = 1, Name = fileName, Path = "path", FileType = ".pdf" };
        _mapperMock.Setup(mapper => mapper.Map<DocumentDAL>(It.IsAny<DocumentDTO>())).Returns(documentDAL);
        _dalValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<DocumentDAL>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
                { new FluentValidation.Results.ValidationFailure("File", "Invalid file.") }));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _documentService.UploadDocumentAsync(fileMock.Object));
    }

    [Fact]
public async Task UpdateDocumentAsync_ShouldUpdateDocument_WhenDocumentExists()
{
    // Arrange
    var documentDTO = new DocumentDTO { Name = "UpdatedDocument", Path = "UpdatedPath", FileType = ".txt" };
    var documentDAL = new DocumentDAL { Id = 1, Name = "Document1", Path = "Path1", FileType = "pdf" };

    // Setup repository mock
    _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(documentDAL);
    
    // Setup validator mock
    _blValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<DocumentBL>(), default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());
    
    // Setup update repository mock (simulates saving the updated document)
    _repositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<DocumentDAL>())).Returns(Task.CompletedTask);

    // Setup AutoMapper mock to map DocumentDTO to DocumentDAL
    _mapperMock.Setup(mapper => mapper.Map(It.IsAny<DocumentDTO>(), It.IsAny<DocumentDAL>()))
        .Callback<DocumentDTO, DocumentDAL>((dto, dal) =>
        {
            dal.Name = dto.Name;
            dal.Path = dto.Path;
            dal.FileType = dto.FileType;
        }).Returns(documentDAL);
    
    // Act
    await _documentService.UpdateDocumentAsync(1, documentDTO);

    // Assert
    Assert.Equal("UpdatedDocument", documentDAL.Name);  // Name should be updated
    Assert.Equal("UpdatedPath", documentDAL.Path);      // Path should be updated
    Assert.Equal(".txt", documentDAL.FileType);         // FileType should be updated
}



    [Fact]
    public async Task UpdateDocumentAsync_ShouldThrowKeyNotFoundException_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentDTO = new DocumentDTO { Name = "UpdatedDocument", Path = "UpdatedPath", FileType = ".txt" };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((DocumentDAL)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _documentService.UpdateDocumentAsync(1, documentDTO));
    }
}
