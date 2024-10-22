using DAL.Data;
using Microsoft.EntityFrameworkCore;
using SharedData.EntitiesDAL;

namespace DAL.Repositories;


public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentContext _context;
    
    public DocumentRepository(DocumentContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<DocumentDAL>> GetAllAsync()
    {
        return await _context.DocumentItems.ToListAsync();
    }
    
    public async Task<DocumentDAL> GetByIdAsync(int id)
    {
        return await _context.DocumentItems.FindAsync(id);
    }
    
    public async Task AddAsync(DocumentDAL item)
    {
        await _context.DocumentItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(DocumentDAL item)
    {
        _context.DocumentItems.Update(item);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var item = await _context.DocumentItems.FindAsync(id);
        _context.DocumentItems.Remove(item);
        await _context.SaveChangesAsync();
    }
}