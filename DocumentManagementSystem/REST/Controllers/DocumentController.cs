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
    /*
    
    [HttpGet]
    public IEnumerable<DocumentDTO> GetDocuments()
    {
        return new List<DocumentDTO>
        {
            new DocumentDTO { Id = 1, Name = "Contract_Agreement_2024.pdf", Path = "/documents/Contract_Agreement_2024.pdf"},
            new DocumentDTO { Id = 2, Name = "Financial_Report_Q3_2024.pdf", Path = "/documents/Financial_Report_Q3_2024.pdf"},
            new DocumentDTO { Id = 3, Name = "Employee_Handbook_2024.pdf", Path = "/documents/Employee_Handbook_2024.pdf"}
        };
    }
    */
    
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

        var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", file.FileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok();
    }
    
    [HttpPost("create-edit")]
    public JsonResult CreateEdit(DocumentDTO document)
    {
        return new JsonResult("Trying to edit document");
    }
}