using DAL.Data;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedData.EntitiesDAL;
using Xunit;
using System;

public class DocumentRepositoryTests
{
    private DocumentContext GetInMemoryDbContext()
    {
        var dbName = Guid.NewGuid().ToString(); // Unique database name per test to isolate them
        var options = new DbContextOptionsBuilder<DocumentContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new DocumentContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDocuments()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.DocumentItems.AddRange(new List<DocumentDAL>
        {
            new DocumentDAL { Id = 1, Name = "Document 1", Path = "/path1", FileType = "pdf" },
            new DocumentDAL { Id = 2, Name = "Document 2", Path = "/path2", FileType = "docx" }
        });
        await context.SaveChangesAsync();

        var repository = new DocumentRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("Document 1", result.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectDocument()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.DocumentItems.AddRange(new List<DocumentDAL>
        {
            new DocumentDAL { Id = 1, Name = "Document 1", Path = "/path1", FileType = "pdf" },
            new DocumentDAL { Id = 2, Name = "Document 2", Path = "/path2", FileType = "docx" }
        });
        await context.SaveChangesAsync();

        var repository = new DocumentRepository(context);

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Document 1", result.Name);
    }

    [Fact]
    public async Task AddAsync_ShouldAddDocument()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repository = new DocumentRepository(context);
        var document = new DocumentDAL { Id = 3, Name = "Document 3", Path = "/path3", FileType = "xlsx" };

        // Act
        await repository.AddAsync(document); // Ensure AddAsync is awaited
        var addedDocument = await context.DocumentItems.FindAsync(3);

        // Assert
        Assert.NotNull(addedDocument);
        Assert.Equal("Document 3", addedDocument.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDocument()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var document = new DocumentDAL { Id = 1, Name = "Document 1", Path = "/path1", FileType = "pdf" };
        context.DocumentItems.Add(document);
        await context.SaveChangesAsync();

        var repository = new DocumentRepository(context);

        // Act
        document.Name = "Updated Document 1";
        await repository.UpdateAsync(document);
        var updatedDocument = await context.DocumentItems.FindAsync(1);

        // Assert
        Assert.Equal("Updated Document 1", updatedDocument.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteDocument()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var document = new DocumentDAL { Id = 1, Name = "Document 1", Path = "/path1", FileType = "pdf" };
        context.DocumentItems.Add(document);
        await context.SaveChangesAsync();

        var repository = new DocumentRepository(context);

        // Act
        await repository.DeleteAsync(1);
        var deletedDocument = await context.DocumentItems.FindAsync(1);

        // Assert
        Assert.Null(deletedDocument);
    }
}
