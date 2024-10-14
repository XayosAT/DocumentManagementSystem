using Microsoft.AspNetCore.Mvc;
using DAL.Repositories;
using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Data;

namespace DAL.Controllers;

[ApiController]
[Route("api/documentitems")]
public class DocumentItemsController : ControllerBase
{
    private readonly IDocumentRepository _repository;

    public DocumentItemsController(IDocumentRepository documentRepository)
    {
        _repository = documentRepository;
    }
    [HttpGet]
    public async Task<IEnumerable<Document>> GetAsync()
    {
        return new List<Document>
        {
            new Document { Id = 1, Name = "Contract_Agreement_2024.pdf", Path = "/documents/Contract_Agreement_2024.pdf"},
            new Document { Id = 2, Name = "Financial_Report_Q3_2024.pdf", Path = "/documents/Financial_Report_Q3_2024.pdf"},
            new Document { Id = 3, Name = "Employee_Handbook_2024.pdf", Path = "/documents/Employee_Handbook_2024.pdf"}
        };
        return await _repository.GetAllAsync();
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(Document item)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            return BadRequest(new { message = "Task name cannot be empty." });
        }
        await _repository.AddAsync(item);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(int id, Document item)
    {
        var existingItem = await _repository.GetByIdAsync(id);
        if (existingItem == null)
        {
            return NotFound();
        }

        existingItem.Name = item.Name;
        existingItem.Path = item.Path;
        existingItem.FileType = item.FileType;
        await _repository.UpdateAsync(existingItem);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
