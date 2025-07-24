using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookService.Services
{
    public interface IBookService
    {
        Task<(bool success, string? error)> RegisterAsync(RegisterDto dto);
        Task<IEnumerable<GetDto>> GetAllBooksAsync();
        Task<GetDto?> GetBookByIdAsync(string id);
        Task<(bool success, string? error)> UpdateBookAsync(string id, RegisterDto dto);
        Task<(bool success, string? error)> DeleteBookAsync(string id); 
    }
}