using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using DAL.Repositories;
using FluentValidation;
using log4net;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.Exceptions;
using SharedData.DTOs;
using SharedData.EntitiesBL;
using SharedData.EntitiesDAL;
using DAL.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Minio.DataModel.Args;
using Elastic.Clients.Elasticsearch;

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
        private readonly IMinioClient _minioClient;
        private readonly ElasticsearchClient _elasticClient;
        private readonly string _bucketName;
        private readonly string _routingKey = "dms_routing_key";

        public DocumentService(
            IDocumentRepository repository,
            IMapper mapper,
            IValidator<DocumentDAL> dalValidator,
            IValidator<DocumentBL> blValidator,
            IMessagePublisher publisher,
            IMinioClient minioClient,
            ElasticsearchClient elasticClient,
            IConfiguration configuration)
        {
            _repository = repository;
            _mapper = mapper;
            _dalValidator = dalValidator;
            _blValidator = blValidator;
            _publisher = publisher;
            _minioClient = minioClient;
            _elasticClient = elasticClient;

            // Set bucket name from configuration or default to 'uploads'.
            _bucketName = configuration["Minio:BucketName"] ?? "uploads";
            InitializeBucket().Wait();

            _logger.Info("DocumentService initialized successfully.");
        }

        // Method to ensure the bucket exists
        private async Task InitializeBucket()
        {
            try
            {
                bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
                if (!found)
                {
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
                    _logger.Info($"Bucket {_bucketName} created successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error while initializing the MinIO bucket.", ex);
                throw;
            }
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

        public async Task<byte[]> GetFileAsync(string filename)
        {
            try
            {
                // Get the file from MinIO
                var ms = new MemoryStream();
                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(filename)
                    .WithCallbackStream((stream) => stream.CopyTo(ms)));

                return ms.ToArray();
            }
            catch (MinioException ex)
            {
                _logger.Error($"Error occurred when trying to fetch file {filename} from MinIO.", ex);
                throw new FileNotFoundException($"File {filename} not found in MinIO.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error occurred when fetching file {filename} from MinIO.", ex);
                throw new FileNotFoundException($"Unexpected error occurred when fetching file {filename}.", ex);
            }
        }

        public async Task<string> UploadDocumentAsync(IFormFile file)
        {
            var fileName = file.FileName;

            try
            {
                // Upload file to MinIO bucket
                await using var stream = file.OpenReadStream();
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType));

                _logger.Info($"File {fileName} successfully uploaded to MinIO bucket {_bucketName}.");

                // Create document DTO and map to DAL entity
                var documentDTO = new DocumentDTO
                {
                    Name = fileName,
                    Path = $"minio://{_bucketName}/{fileName}",
                    FileType = Path.GetExtension(fileName)
                };

                var documentDAL = _mapper.Map<DocumentDAL>(documentDTO);

                // Validate the DAL entity using FluentValidation
                var validationResult = await _dalValidator.ValidateAsync(documentDAL);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                await _repository.AddAsync(documentDAL);
                _publisher.Publish(documentDAL.Path, _routingKey);

                // Index the document in Elasticsearch
                var indexResponse = await _elasticClient.IndexAsync(documentDTO, i => i.Index("documents"));
                if (!indexResponse.IsValidResponse)
                {
                    _logger.Error("Failed to index document in Elasticsearch.");
                }

                return documentDAL.Id.ToString();
            }
            catch (Exception ex)
            {
                _logger.Error("An unexpected error occurred when uploading the document.", ex);
                throw;
            }
        }

        public async Task UpdateDocumentAsync(int id, DocumentDTO documentDTO)
        {
            try
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
            catch (ValidationException ex)
            {
                _logger.Error("Validation error during document update.", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred while updating document with ID: {id}", ex);
                throw;
            }
        }

        public async Task DeleteDocumentAsync(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException("Document not found.");
            }

            try
            {
                // Delete from MinIO bucket
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_bucketName).WithObject(item.Name));
                _logger.Info($"File {item.Name} successfully removed from MinIO bucket {_bucketName}.");
            }
            catch (MinioException ex)
            {
                _logger.Error($"Error occurred when deleting file {item.Name} from MinIO.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred when deleting file {item.Name} from MinIO.", ex);
            }

            try
            {
                await _repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred when deleting document with ID: {id}", ex);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentDTO>> SearchDocumentsAsync(string query)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<DocumentDTO>(s => s
                    .Index("documents")
                    .Query(q => q
                        .MultiMatch(m => m
                            .Fields(new[] { "name", "fileType", "path" }) // Specify fields as a string array
                            .Query(query)
                        )
                    )
                );

                if (!searchResponse.IsValidResponse)
                {
                    throw new Exception("Search operation failed in Elasticsearch.");
                }

                return searchResponse.Documents;
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurred while performing search in Elasticsearch.", ex);
                throw;
            }
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
