using Microsoft.AspNetCore.Mvc;
using DAL.Repositories;
using SharedData.DTOs;
using FluentValidation;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using REST.RabbitMQ;
using SharedData.EntitiesBL;
using SharedData.EntitiesDAL;

namespace REST.Controllers;

[ApiController]
[Route("document")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<DocumentDAL> _dalValidator;
    private readonly IValidator<DocumentBL> _blValidator;
    private readonly string _uploadFolder;
    private readonly IMessagePublisher _publisher;

    public DocumentController(
        IDocumentRepository documentRepository, 
        IMapper mapper, 
        IValidator<DocumentDAL> dalValidator,
        IValidator<DocumentBL> blValidator,
        IMessagePublisher publisher)
    {
        _repository = documentRepository;
        _mapper = mapper;
        _dalValidator = dalValidator;
        _blValidator = blValidator;
        _publisher = publisher;

        // Initialize the upload folder path
        _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        if (!Directory.Exists(_uploadFolder))
        {
            Directory.CreateDirectory(_uploadFolder);
        }
    }

    // GET: document
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var documents = await _repository.GetAllAsync();
        var documentDTOs = _mapper.Map<IEnumerable<DocumentDTO>>(documents);
        return Ok(documentDTOs);
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

        // Determine the content type based on the file extension
        string contentType = GetContentType(filePath);

        // Read the file content and return it as a response
        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, contentType);
    }

    // POST: document/upload
    [HttpPost("upload")]
    public async Task<IActionResult> Post([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is null or empty");
        }

        // Save file to the uploads directory
        var path = Path.Combine(_uploadFolder, file.FileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Create document DTO and map to DAL entity
        var documentDTO = new DocumentDTO
        {
            Name = file.FileName,
            Path = path,
            FileType = Path.GetExtension(file.FileName)
        };

        var documentDAL = _mapper.Map<DocumentDAL>(documentDTO);

        // Validate the DAL entity using FluentValidation
        var validationResult = await _dalValidator.ValidateAsync(documentDAL);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        await _repository.AddAsync(documentDAL);
        _publisher.Publish($"Document Path: {documentDAL.Path}", "dms_routing_key");
        
        // this is causing an error but the program does not crash
        return CreatedAtAction(nameof(GetAsync), new { id = documentDAL.Id }, documentDAL);
    }

    // DELETE: document/{id}
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

    // PUT: document/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] DocumentDTO documentDTO)
    {
        if (documentDTO == null)
        {
            return BadRequest("Invalid document data");
        }

        var existingItem = await _repository.GetByIdAsync(id);
        if (existingItem == null)
        {
            return NotFound();
        }
        
        var documentBL = _mapper.Map<DocumentBL>(documentDTO);
        var validationResult = await _blValidator.ValidateAsync(documentBL);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        _mapper.Map(documentDTO, existingItem);
        

        await _repository.UpdateAsync(existingItem);
        return NoContent();
    }

    // Helper method to get content type based on file extension
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
}
