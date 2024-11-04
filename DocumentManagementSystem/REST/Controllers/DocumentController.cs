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
using log4net;

namespace REST.Controllers;

[ApiController]
[Route("document")]
public class DocumentController : ControllerBase
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DocumentController));
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
        _logger.Info("DocumentController initialized successfully.");
    }

    // GET: document
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        _logger.Info("Received GET request for all documents.");
        try
        {
            var documents = await _repository.GetAllAsync();
            var documentDTOs = _mapper.Map<IEnumerable<DocumentDTO>>(documents);
            _logger.Info($"Successfully retrieved {documentDTOs?.Count()} documents.");
            return Ok(documentDTOs);
        }
        catch (Exception ex)
        {
            _logger.Error("An error occurred while getting documents.", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving documents.");
        }
    }

    // GET: document/files/{filename}
    [HttpGet("files/{filename}")]
    public IActionResult GetFile(string filename)
    {
        _logger.Info($"Received GET request for file: {filename}");
        var filePath = Path.Combine(_uploadFolder, filename);

        if (!System.IO.File.Exists(filePath))
        {
            _logger.Warn($"File not found: {filename}");
            return NotFound("File not found.");
        }

        string contentType = GetContentType(filePath);
        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        _logger.Info($"Successfully retrieved file: {filename}");
        return File(fileBytes, contentType);
    }

    // POST: document/upload
    [HttpPost("upload")]
    public async Task<IActionResult> Post([FromForm] IFormFile file)
    {
        _logger.Info("Received file upload request.");

        if (file == null || file.Length == 0)
        {
            _logger.Warn("Invalid file upload request. File is null or empty.");
            return BadRequest("File is null or empty");
        }

        try
        {
            var path = Path.Combine(_uploadFolder, file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.Info($"File {file.FileName} successfully saved to {path}");

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
                _logger.Warn("Validation failed for DocumentDAL entity.");
                return BadRequest(validationResult.Errors);
            }

            await _repository.AddAsync(documentDAL);
            _publisher.Publish($"Document Path: {documentDAL.Path}", "dms_routing_key");
            _logger.Info("Document metadata successfully saved to the database.");

            // Use Ok with a message instead of CreatedAtAction to avoid routing issues
            return Ok(new { message = "Document successfully uploaded.", documentId = documentDAL.Id });
        }
        catch (Exception ex)
        {
            _logger.Error("An error occurred while uploading the document.", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while uploading the document.");
        }
    }

    // DELETE: document/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        _logger.Info($"Received DELETE request for document id: {id}");

        try
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
            {
                _logger.Warn($"Document with id {id} not found.");
                return NotFound();
            }

            await _repository.DeleteAsync(id);
            _logger.Info($"Document with id {id} successfully deleted.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while deleting the document with id {id}.", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the document.");
        }
    }

    // PUT: document/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] DocumentDTO documentDTO)
    {
        _logger.Info($"Received PUT request for document id: {id}");

        if (documentDTO == null)
        {
            _logger.Warn("Invalid document data received for update.");
            return BadRequest("Invalid document data");
        }

        try
        {
            var existingItem = await _repository.GetByIdAsync(id);
            if (existingItem == null)
            {
                _logger.Warn($"Document with id {id} not found.");
                return NotFound();
            }

            // Map DTO to Business Logic Entity and validate
            var documentBL = _mapper.Map<DocumentBL>(documentDTO);
            var blValidationResult = await _blValidator.ValidateAsync(documentBL);
            if (!blValidationResult.IsValid)
            {
                _logger.Warn("Validation failed for DocumentBL entity.");
                return BadRequest(blValidationResult.Errors);
            }

            // Map DTO directly to the existing DAL entity
            _mapper.Map(documentDTO, existingItem);
            await _repository.UpdateAsync(existingItem);

            _logger.Info($"Document with id {id} successfully updated.");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while updating the document with id {id}.", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the document.");
        }
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
