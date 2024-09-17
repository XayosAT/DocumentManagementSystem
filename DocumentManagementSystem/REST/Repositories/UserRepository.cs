using DocumentManagementSystem.Entities;

namespace DocumentManagementSystem.Repositories;

public interface IUserRepository
{
    Task<User> GetUserById(int id);
    Task AddUser(User user);
}

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<User> GetUserById(int id)
    {
        return await _context.Users.FindAsync(id);
    }
    
    public async Task AddUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
}