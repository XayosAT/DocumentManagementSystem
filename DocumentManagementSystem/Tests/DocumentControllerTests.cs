using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedData.DTOs;
using REST.Controllers;
using DAL.Repositories;
using AutoMapper;
using FluentValidation;
using SharedData.EntitiesDAL;
using SharedData.EntitiesBL;
using Microsoft.AspNetCore.Http;
using System.IO;
using REST.RabbitMQ;

public class DocumentControllerTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<DocumentDAL>> _dalValidatorMock;
    private readonly Mock<IValidator<DocumentBL>> _blValidatorMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly DocumentController _controller;

    public DocumentControllerTests()
    {
        _repositoryMock = new Mock<IDocumentRepository>();
        _mapperMock = new Mock<IMapper>();
        _dalValidatorMock = new Mock<IValidator<DocumentDAL>>();
        _blValidatorMock = new Mock<IValidator<DocumentBL>>();
        _publisherMock = new Mock<IMessagePublisher>();

        _controller = new DocumentController(
            _repositoryMock.Object,
            _mapperMock.Object,
            _dalValidatorMock.Object,
            _blValidatorMock.Object,
            _publisherMock.Object
        );
    }

    [Fact]
    public async Task Get_ShouldReturnAllDocuments_WhenResponseIsSuccessful()
    {
        // Arrange
        var documentList = new List<DocumentDAL>
        {
            new DocumentDAL { Id = 1, Name = "Document1", Path = "Path1", FileType = "pdf" },
            new DocumentDAL { Id = 2, Name = "Document2", Path = "Path2", FileType = "docx" }
        };

        _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(documentList);

        var documentDTOList = new List<DocumentDTO>
        {
            new DocumentDTO { Name = "Document1", Path = "Path1", FileType = "pdf" },
            new DocumentDTO { Name = "Document2", Path = "Path2", FileType = "docx" }
        };

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<DocumentDTO>>(It.IsAny<IEnumerable<DocumentDAL>>()))
            .Returns(documentDTOList);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDocuments = Assert.IsType<List<DocumentDTO>>(okResult.Value);
        Assert.Equal(2, returnedDocuments.Count);
    }

    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenDocumentsDoNotExist()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync((IEnumerable<DocumentDAL>)null);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);  // Ensure OkObjectResult is returned
        var returnedList = Assert.IsType<DocumentDTO[]>(okResult.Value);  // Ensure the result is a list
        Assert.Empty(returnedList); 
    }
    /*
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

        var documentDAL = new DocumentDAL { Name = fileName, Path = "Uploads/test.txt", FileType = ".txt", Id = 1 };

        _mapperMock.Setup(mapper => mapper.Map<DocumentDAL>(It.IsAny<DocumentDTO>())).Returns(documentDAL);
        _dalValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<DocumentDAL>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<DocumentDAL>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Post(fileMock.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Extract the value as a dictionary
        var responseObject = Assert.IsType<Dictionary<string, object>>(okResult.Value);
    
        // Assert values in the dictionary
        Assert.Equal("Document successfully uploaded.", responseObject["message"]);
        Assert.Equal(documentDAL.Id, (int)responseObject["documentId"]);
    }
    */

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

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenDocumentIsDeleted()
    {
        // Arrange
        var documentDAL = new DocumentDAL { Id = 1, Name = "Document1", Path = "Path1", FileType = "pdf" };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(documentDAL);
        _repositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((DocumentDAL)null);

        // Act
        var result = await _controller.DeleteAsync(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
