using userService.DTOs.User;

namespace userService.Services
{
    public interface IUserService
    {
        Task<(bool success, string token, string? error)> RegisterAsync(RegisterDto dto);
        Task<(bool success, string token, string? error)> LoginAsync(LoginDto dto);
        Task<IEnumerable<GetDto>> GetAllUsersAsync();
        Task<GetDto?> GetUserByIdAsync(int id);
        Task<(bool success, string? error)> UpdateUserProfileAsync(int userId, UpdateProfileDto dto);
    }
} 