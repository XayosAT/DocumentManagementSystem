using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DocumentManagementSystem.Models;

namespace DocumentManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{   
    [HttpGet]
    public IEnumerable<Document> Get()
    {
        return new List<Document>
        {
            new Document { Id = 1, Name = "Document 1", Path = "/documents/1" },
            new Document { Id = 2, Name = "Document 2", Path = "/documents/2" },
            new Document { Id = 3, Name = "Document 3", Path = "/documents/3" }
        };
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Post([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is null or empty");
        }

        var path = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.FileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok();
    }
    
    [HttpPost("create-edit")]
    public JsonResult CreateEdit(Document document)
    {
        return new JsonResult("Trying to edit document");
    }
}