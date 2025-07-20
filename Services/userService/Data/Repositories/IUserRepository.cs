using userService.Models;
using userService.DTOs.User;

namespace userService.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<GetDto>> GetAllUsersAsync();
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task SaveChangesAsync();
    }
} 