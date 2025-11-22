using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookService.ExternalProviders
{
    public interface IExternalBookProvider
    {
        Task<IEnumerable<ExternalBookSearchDto>> SearchBooksAsync(string query, int limit = 10);
        Task<ExternalBookDetailDto?> GetBookByIsbnAsync(string isbn);
        Task<ExternalBookDetailDto?> GetBookByIdAsync(string id);
    }
}