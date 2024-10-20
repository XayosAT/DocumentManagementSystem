using DocumentManagementSystem.Entities;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;


public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentContext _context;
    
    public DocumentRepository(DocumentContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        return await _context.DocumentItems.ToListAsync();
    }
    
    public async Task<Document> GetByIdAsync(int id)
    {
        return await _context.DocumentItems.FindAsync(id);
    }
    
    public async Task AddAsync(Document item)
    {
        _context.DocumentItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Document item)
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