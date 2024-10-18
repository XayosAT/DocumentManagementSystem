using Microsoft.AspNetCore.Mvc;
using DAL.Repositories;
using DocumentManagementSystem.Entities;
using DocumentManagementSystem.DTOs;
using FluentValidation;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;

namespace DAL.Controllers;

[ApiController]
[Route("api/documentitems")]
public class DocumentItemsController : ControllerBase
{
    private readonly IDocumentRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<DocumentDTO> _validator;

    public DocumentItemsController(IDocumentRepository documentRepository, IMapper mapper, IValidator<DocumentDTO> validator)
    {
        _repository = documentRepository;
        _mapper = mapper;
        _validator = validator;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var documents = await _repository.GetAllAsync();
        var documentDTOs = _mapper.Map<IEnumerable<DocumentDTO>>(documents);
        return Ok(documentDTOs);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] DocumentDTO item)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            return BadRequest(new { message = "Document name cannot be empty." });
        }
        
        var validationResult = await _validator.ValidateAsync(item);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        var document = _mapper.Map<Document>(item);
        await _repository.AddAsync(document);
        return CreatedAtAction(nameof(GetAsync), new { id = document.Id }, document);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(int id, [FromBody] DocumentDTO item)
    {
        var existingItem = await _repository.GetByIdAsync(id);
        if (existingItem == null)
        {
            return NotFound();
        }
        var validationResult = await _validator.ValidateAsync(item);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        var document = _mapper.Map<Document>(item);
        existingItem.Name = document.Name;
        existingItem.Path = document.Path;
        existingItem.FileType = document.FileType;
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