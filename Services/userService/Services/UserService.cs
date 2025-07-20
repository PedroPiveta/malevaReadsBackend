using System.Security.Cryptography;
using System.Text;
using userService.Data.Repositories;
using userService.DTOs.User;
using userService.Models;

namespace userService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;

        public UserService(IUserRepository userRepository, TokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<(bool success, string token, string? error)> RegisterAsync(RegisterDto dto)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email))
                return (false, string.Empty, "Email já registrado.");

            CreatePasswordHash(dto.Password, out byte[] hash, out byte[] salt);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PasswordHash = hash,
                PasswordSalt = salt,
            };

            await _userRepository.CreateAsync(user);
            var token = _tokenService.GenerateToken(user);
            
            return (true, token, null);
        }

        public async Task<(bool success, string token, string? error)> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user is null || !VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt))
                return (false, string.Empty, "Email ou senha inválidos.");

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            
            var token = _tokenService.GenerateToken(user);
            return (true, token, null);
        }

        public async Task<IEnumerable<GetDto>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<GetDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new GetDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePicUrl = user.ProfilePicUrl,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                Role = user.Role,
            };
        }

        public async Task<(bool success, string? error)> UpdateUserProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, "Usuário não encontrado.");

            user.Username = dto.Username;
            await _userRepository.UpdateAsync(user);
            
            return (true, null);
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash);
        }
    }
} 