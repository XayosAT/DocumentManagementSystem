using SharedData.EntitiesDAL;

namespace DAL.Repositories;

public interface IDocumentRepository
{
    Task<IEnumerable<DocumentDAL>> GetAllAsync();
    Task<DocumentDAL> GetByIdAsync(int id);
    Task AddAsync(DocumentDAL item);
    Task UpdateAsync(DocumentDAL item);
    Task DeleteAsync(int id);
}