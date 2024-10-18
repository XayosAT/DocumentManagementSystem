using Microsoft.AspNetCore.Mvc;
using DAL.Repositories;
using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    public async Task<IActionResult> GetAsync()
    {
        var documents = await _repository.GetAllAsync();
        return Ok(documents);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] Document item)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            return BadRequest(new { message = "Document name cannot be empty." });
        }
        await _repository.AddAsync(item);
        return CreatedAtAction(nameof(GetAsync), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(int id, [FromBody] Document item)
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