using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class DocumentControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DocumentControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllDocuments_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/document/getall");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(content));
    }

    [Fact]
    public async Task UploadDocument_ShouldReturnOk_WhenFileIsValid()
    {
        // Arrange
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Dummy file content"));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", "testfile.txt");

        // Act
        var response = await _client.PostAsync("/document/upload", formData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("Document successfully uploaded", responseBody);
    }

    [Fact]
    public async Task UpdateDocument_ShouldReturnNoContent_WhenDataIsValid()
    {
        // Arrange
        var documentId = 1; // Replace with an existing ID for the test
        var updatedDocument = new
        {
            Id = documentId,
            Name = "Updated Document Name",
            Path = "Updated/Path",
            FileType = ".txt"
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(updatedDocument), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/document/{documentId}", jsonContent);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDocument_ShouldReturnNoContent_WhenDocumentExists()
    {
        // Arrange
        var documentId = 1; // Replace with an existing ID for the test

        // Act
        var response = await _client.DeleteAsync($"/document/{documentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDocument_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = 99999; // Use a non-existent ID

        // Act
        var response = await _client.DeleteAsync($"/document/{documentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
