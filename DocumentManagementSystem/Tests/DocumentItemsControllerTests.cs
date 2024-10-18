using Xunit;
using Moq;
using DAL.Repositories;
using DAL.Entities;
using DAL.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DocumentItemsControllerTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly DocumentItemsController _controller;

    public DocumentItemsControllerTests()
    {
        // Step 1: Create a Mock of IDocumentRepository
        _documentRepositoryMock = new Mock<IDocumentRepository>();

        // Step 2: Inject Mock into Controller
        _controller = new DocumentItemsController(_documentRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnAllDocuments()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { Id = 1, Name = "Document1", Path = "Path1", FileType = ".pdf" },
            new Document { Id = 2, Name = "Document2", Path = "Path2", FileType = ".docx" }
        };
        
        _documentRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(documents);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result); // Assert that the response is OkObjectResult
        var returnedDocuments = Assert.IsType<List<Document>>(okResult.Value); // Assert that the value inside OkObjectResult is a List<Document>
        Assert.Equal(2, returnedDocuments.Count); // Verify the number of documents
    }

    [Fact]
    public async Task PostAsync_ShouldAddDocument_WhenValidDocumentIsProvided()
    {
        // Arrange
        var newDocument = new Document { Id = 3, Name = "Document3", Path = "Path3", FileType = ".txt" };

        _documentRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Document>()))
            .Returns(Task.CompletedTask); // This simulates a successful add operation

        // Act
        var result = await _controller.PostAsync(newDocument);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result); // Assert that the response is CreatedAtActionResult
        var createdDocument = Assert.IsType<Document>(createdAtResult.Value); // Assert that the value inside CreatedAtActionResult is a Document
        Assert.Equal(newDocument.Name, createdDocument.Name);
        _documentRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Document>(d => d.Name == newDocument.Name)), Times.Once);
    }

    [Fact]
    public async Task PutAsync_ShouldUpdateDocument_WhenDocumentExists()
    {
        // Arrange
        var existingDocument = new Document { Id = 1, Name = "ExistingDocument", Path = "Path1", FileType = ".pdf" };
        var updatedDocument = new Document { Id = 1, Name = "UpdatedDocument", Path = "Path1", FileType = ".pdf" };

        _documentRepositoryMock
            .Setup(repo => repo.GetByIdAsync(existingDocument.Id))
            .ReturnsAsync(existingDocument);
        
        _documentRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Document>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.PutAsync(existingDocument.Id, updatedDocument);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result); // Assert that the response is NoContentResult
        _documentRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Document>(d => d.Name == updatedDocument.Name)), Times.Once);
    }

    [Fact]
    public async Task PutAsync_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var updatedDocument = new Document { Id = 99, Name = "UpdatedDocument", Path = "Path1", FileType = ".pdf" };

        _documentRepositoryMock
            .Setup(repo => repo.GetByIdAsync(updatedDocument.Id))
            .ReturnsAsync((Document)null); // Simulate document not found

        // Act
        var result = await _controller.PutAsync(updatedDocument.Id, updatedDocument);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result); // Assert that the response is NotFoundResult
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteDocument_WhenDocumentExists()
    {
        // Arrange
        var existingDocument = new Document { Id = 1, Name = "DocumentToDelete", Path = "Path1", FileType = ".pdf" };

        _documentRepositoryMock
            .Setup(repo => repo.GetByIdAsync(existingDocument.Id))
            .ReturnsAsync(existingDocument);
        
        _documentRepositoryMock
            .Setup(repo => repo.DeleteAsync(existingDocument.Id))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAsync(existingDocument.Id);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result); // Assert that the response is NoContentResult
        _documentRepositoryMock.Verify(repo => repo.DeleteAsync(existingDocument.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = 99;

        _documentRepositoryMock
            .Setup(repo => repo.GetByIdAsync(documentId))
            .ReturnsAsync((Document)null); // Simulate document not found

        // Act
        var result = await _controller.DeleteAsync(documentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result); // Assert that the response is NotFoundResult
    }
}
