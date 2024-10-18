using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DocumentManagementSystem.DTOs;
using System.Collections.Generic;

namespace DocumentManagementSystem.Controllers;

[ApiController]
[Route("document")]
public class DocumentController : ControllerBase
{
    private readonly string _uploadFolder;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public DocumentController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        if (!Directory.Exists(_uploadFolder))
        {
            Directory.CreateDirectory(_uploadFolder);
        }
    }
    
    [HttpGet]
    private string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".json" => "application/json",
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var client = _httpClientFactory.CreateClient("DAL");
        var response = await client.GetAsync("/api/documentitems");
        
        if (response.IsSuccessStatusCode) 
        {
            var documents = await response.Content.ReadFromJsonAsync<IEnumerable<DocumentDTO>>();
            return Ok(documents);
        }
        return StatusCode((int)response.StatusCode, "Failed to retrieve documents");
    }
    
    // GET: document/files/{filename}
    [HttpGet("files/{filename}")]
    public IActionResult GetFile(string filename)
    {
        // Combine the uploads folder with the requested filename
        var filePath = Path.Combine(_uploadFolder, filename);

        // Check if the file exists
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        // Determine the content type based on the file extension (for example: "application/json" for .json files)
        string contentType = GetContentType(filePath);

        // Read the file content and return it as a response
        var fileBytes = System.IO.File.ReadAllBytes(filePath);

        // Instead of forcing a download, return the file with the correct content type so that it opens in the browser
        return File(fileBytes, contentType);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Post([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is null or empty");
        }

        // Save file to the uploads directory
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }
        var path = Path.Combine(uploadPath, file.FileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Save file metadata to the database
        var document = new DocumentDTO
        {
            Name = file.FileName,
            Path = path,
            FileType = Path.GetExtension(file.FileName)
        };

        var client = _httpClientFactory.CreateClient("DAL");
        var response = await client.PostAsJsonAsync("/api/documentitems", document);
    
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Failed to save document metadata");
        }

        return Ok();
    }

    
    
    [HttpPost("create-edit")]
    public JsonResult CreateEdit(DocumentDTO document)
    {
        return new JsonResult("Trying to edit document");
    }
}