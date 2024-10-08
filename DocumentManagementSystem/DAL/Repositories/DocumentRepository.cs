using DAL.Entities;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;


public class DocumentRepository(DocumentContext context) : IDocumentRepository
{
    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        return await context.DocumentItems.ToListAsync();
    }
    
    public async Task<Document> GetByIdAsync(int id)
    {
        return await context.DocumentItems.FindAsync(id);
    }
    
    public async Task AddAsync(Document item)
    {
        await context.DocumentItems.AddAsync(item);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Document item)
    {
        context.DocumentItems.Update(item);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var item = await context.DocumentItems.FindAsync(id);
        context.DocumentItems.Remove(item);
        await context.SaveChangesAsync();
    }
}