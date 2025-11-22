using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookService.Services
{
    public interface IExternalBookService
    {
        Task<IEnumerable<ExternalBookSearchDto>> SearchBooksAsync(string query);
        Task<ExternalBookDetailDto?> GetBookDetailsAsync(string externalId, string provider);
        Task<ExternalBookDetailDto?> GetBookByIsbnAsync(string isbn);
        Task<(bool success, string? bookId, string? error)> ImportBookAsync(string externalId, string provider);
    }
}