using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using DAL.Repositories;
using DAL.Services;
using FluentValidation;
using log4net;
using Microsoft.AspNetCore.Http;
using SharedData.DTOs;
using SharedData.EntitiesBL;
using SharedData.EntitiesDAL;
using DAL.RabbitMQ;

namespace DAL.Services
{
    public class DocumentService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DocumentService));
        private readonly IDocumentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IValidator<DocumentDAL> _dalValidator;
        private readonly IValidator<DocumentBL> _blValidator;
        private readonly IMessagePublisher _publisher;
        private readonly string _uploadFolder;

        public DocumentService(
            IDocumentRepository repository,
            IMapper mapper,
            IValidator<DocumentDAL> dalValidator,
            IValidator<DocumentBL> blValidator,
            IMessagePublisher publisher)
        {
            _repository = repository;
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
            _logger.Info("DocumentService initialized successfully.");
        }

        public async Task<IEnumerable<DocumentDTO>> GetAllDocumentsAsync()
        {
            var documents = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<DocumentDTO>>(documents);
        }

        public async Task<DocumentDTO> GetDocumentByIdAsync(int id)
        {
            var document = await _repository.GetByIdAsync(id);
            return document != null ? _mapper.Map<DocumentDTO>(document) : null;
        }

        public byte[] GetFile(string filename, out string contentType)
        {
            var filePath = Path.Combine(_uploadFolder, filename);
            if (!File.Exists(filePath))
            {
                contentType = null;
                return null;
            }

            contentType = GetContentType(filePath);
            return File.ReadAllBytes(filePath);
        }

        public async Task<string> UploadDocumentAsync(IFormFile file)
        {
            var path = Path.Combine(_uploadFolder, file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var documentDTO = new DocumentDTO
            {
                Name = file.FileName,
                Path = path,
                FileType = Path.GetExtension(file.FileName)
            };

            var documentDAL = _mapper.Map<DocumentDAL>(documentDTO);

            var validationResult = await _dalValidator.ValidateAsync(documentDAL);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _repository.AddAsync(documentDAL);
            _publisher.Publish($"Document Path: {documentDAL.Path}", "dms_routing_key");

            return documentDAL.Id.ToString();
        }

        public async Task UpdateDocumentAsync(int id, DocumentDTO documentDTO)
        {
            var existingItem = await _repository.GetByIdAsync(id);
            if (existingItem == null)
            {
                throw new KeyNotFoundException("Document not found.");
            }

            var documentBL = _mapper.Map<DocumentBL>(documentDTO);
            var validationResult = await _blValidator.ValidateAsync(documentBL);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            _mapper.Map(documentDTO, existingItem);
            await _repository.UpdateAsync(existingItem);
        }

        public async Task DeleteDocumentAsync(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException("Document not found.");
            }

            await _repository.DeleteAsync(id);
        }

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
}
