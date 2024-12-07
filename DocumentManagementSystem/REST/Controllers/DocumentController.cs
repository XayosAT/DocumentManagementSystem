using Microsoft.AspNetCore.Mvc;
using DAL.Services;
using SharedData.DTOs;
using log4net;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentValidation;

namespace REST.Controllers;

[ApiController]
[Route("document")]
public class DocumentController : ControllerBase
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DocumentController));
    private readonly DocumentService _documentService;
    

    public DocumentController(DocumentService documentService)
    {
        _documentService = documentService;
        _logger.Info("DocumentController initialized successfully.");
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAsync()
    {
        _logger.Info("Received GET request for all documents.");
        try
        {
            var documents = await _documentService.GetAllDocumentsAsync();
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.Error("An error occurred while fetching all documents.", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching documents.");
        }
    }

    // [HttpGet("files/{filename}")]
    // public IActionResult GetFile(string filename)
    // {
    //     _logger.Info($"Received GET request for file: {filename}");
    //     var fileBytes = _documentService.GetFile(filename, out string contentType);
    //     if (fileBytes == null)
    //     {
    //         return NotFound("File not found.");
    //     }
    //     return File(fileBytes, contentType);
    // }

    [HttpPost("upload")]
    public async Task<IActionResult> Post([FromForm] IFormFile file)
    {
        _logger.Info("Received file upload request.");
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is null or empty");
        }

        try
        {
            var documentId = await _documentService.UploadDocumentAsync(file);
            return Ok(new { message = "Document successfully uploaded.", documentId });
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (Exception ex) // Catch any other unexpected errors
        {
            _logger.Error("An unexpected error occurred while uploading the document.", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while uploading the document.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        _logger.Info($"Received DELETE request for document id: {id}");
        try
        {
            await _documentService.DeleteDocumentAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex) // Handle unexpected errors
        {
            _logger.Error($"An error occurred while deleting document with ID: {id}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the document.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] DocumentDTO documentDTO)
    {
        _logger.Info($"Received PUT request for document id: {id}");
        if (documentDTO == null)
        {
            return BadRequest("Invalid document data");
        }

        try
        {
            await _documentService.UpdateDocumentAsync(id, documentDTO);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex) // Handle unexpected errors
        {
            _logger.Error($"An error occurred while updating document with ID: {id}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the document.");
        }
    }
    [HttpGet("search")]
    public async Task<IActionResult> SearchAsync([FromQuery] string query)
    {
        _logger.Info($"Received search request with query: {query}");
        if (string.IsNullOrEmpty(query))
        {
            return BadRequest("Query cannot be empty.");
        }

        try
        {
            var results = await _documentService.SearchDocumentsAsync(query);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.Error("Search failed.", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while performing the search.");
        }
    }

}
