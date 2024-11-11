using DAL.Data;
using Microsoft.EntityFrameworkCore;
using SharedData.EntitiesDAL;
using log4net;
using System.Reflection;

namespace DAL.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        // Create a logger instance for this class
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        
        private readonly DocumentContext _context;

        public DocumentRepository(DocumentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<DocumentDAL>> GetAllAsync()
        {
            try
            {
                _logger.Info("Fetching all documents from the database.");
                var documents = await _context.DocumentItems.ToListAsync();
                _logger.Info($"{documents.Count} documents fetched successfully.");
                return documents;
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while fetching all documents.", ex);
                throw; // re-throwing the exception for higher-level handling
            }
        }

        public async Task<DocumentDAL> GetByIdAsync(int id)
        {
            try
            {
                _logger.Info($"Fetching document with ID: {id}.");
                var document = await _context.DocumentItems.FindAsync(id);
                
                if (document == null)
                {
                    _logger.Warn($"Document with ID: {id} was not found.");
                }
                else
                {
                    _logger.Info($"Document with ID: {id} fetched successfully.");
                }

                return document;
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while fetching document with ID: {id}.", ex);
                throw;
            }
        }

        public async Task AddAsync(DocumentDAL item)
        {
            try
            {
                _logger.Info($"Adding a new document with Name: {item.Name}.");
                await _context.DocumentItems.AddAsync(item);
                await _context.SaveChangesAsync();
                _logger.Info($"Document with ID: {item.Id} added successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while adding a new document: {item.Name}.", ex);
                throw;
            }
        }

        public async Task UpdateAsync(DocumentDAL item)
        {
            try
            {
                _logger.Info($"Updating document with ID: {item.Id}.");
                _context.DocumentItems.Update(item);
                await _context.SaveChangesAsync();
                _logger.Info($"Document with ID: {item.Id} updated successfully.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Error($"A concurrency error occurred while updating document with ID: {item.Id}.", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while updating document with ID: {item.Id}.", ex);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _logger.Info($"Attempting to delete document with ID: {id}.");
                var item = await _context.DocumentItems.FindAsync(id);
                if (item == null)
                {
                    _logger.Warn($"Document with ID: {id} not found, delete operation skipped.");
                    return;
                }

                _context.DocumentItems.Remove(item);
                await _context.SaveChangesAsync();
                _logger.Info($"Document with ID: {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while deleting document with ID: {id}.", ex);
                throw;
            }
        }
    }
}
