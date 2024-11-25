using Microsoft.EntityFrameworkCore;
using SharedData.EntitiesDAL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using DAL.Repositories;
using DAL.Data;

public class DocumentRepositoryTests
{
    private readonly DocumentContext _context;
    private readonly DocumentRepository _repository;

    public DocumentRepositoryTests()
    {
        // Setting up an in-memory database
        var options = new DbContextOptionsBuilder<DocumentContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for every test instance
            .Options;

        _context = new DocumentContext(options);
        _repository = new DocumentRepository(_context);

        // Seed the database with initial data
        SeedDatabase();
    }

    // Helper method to seed the in-memory database
    private void SeedDatabase()
    {
        _context.DocumentItems.AddRange(
            new DocumentDAL { Id = 1, Name = "Document1", Path = "Path1", FileType = ".txt" },
            new DocumentDAL { Id = 2, Name = "Document2", Path = "Path2", FileType = ".pdf" }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDocuments()
    {
        // Act
        var documents = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(documents);
        Assert.Equal(2, ((List<DocumentDAL>)documents).Count);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDocument_WhenDocumentExists()
    {
        // Act
        var document = await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(document);
        Assert.Equal(1, document.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
    {
        // Act
        var document = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(document);
    }

    [Fact]
    public async Task AddAsync_ShouldAddDocument()
    {
        // Arrange
        var newDocument = new DocumentDAL { Id = 3, Name = "Document3", Path = "Path3", FileType = ".docx" };

        // Act
        await _repository.AddAsync(newDocument);

        // Assert
        var document = await _repository.GetByIdAsync(3);
        Assert.NotNull(document);
        Assert.Equal("Document3", document.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDocument()
    {
        // Arrange
        var document = await _repository.GetByIdAsync(1);
        document.Name = "UpdatedName";

        // Act
        await _repository.UpdateAsync(document);

        // Assert
        var updatedDocument = await _repository.GetByIdAsync(1);
        Assert.Equal("UpdatedName", updatedDocument.Name);
    }


    [Fact]
    public async Task DeleteAsync_ShouldDeleteDocument_WhenDocumentExists()
    {
        // Arrange
        int documentId = 1;

        // Act
        await _repository.DeleteAsync(documentId);

        // Assert
        var document = await _repository.GetByIdAsync(documentId);
        Assert.Null(document);
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenDocumentDoesNotExist()
    {
        // Arrange
        int documentId = 999;

        // Act & Assert
        await _repository.DeleteAsync(documentId); // Should not throw an exception
    }
}
