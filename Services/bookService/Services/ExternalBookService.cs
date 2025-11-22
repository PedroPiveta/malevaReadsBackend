using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bookService.ExternalProviders;

namespace bookService.Services
{
    public class ExternalBookService : IExternalBookService
    {
        private readonly IExternalBookProvider _provider;
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<ExternalBookService> _logger;

        public ExternalBookService(
            IExternalBookProvider provider,
            IBookRepository bookRepository,
            ILogger<ExternalBookService> logger)
        {
            _provider = provider;
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ExternalBookSearchDto>> SearchBooksAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<ExternalBookSearchDto>();

            return await _provider.SearchBooksAsync(query);
        }

        public async Task<ExternalBookDetailDto?> GetBookDetailsAsync(string externalId, string provider)
        {
            if (provider.ToLower() != "openlibrary")
                return null;

            return await _provider.GetBookByIdAsync(externalId);
        }

        public async Task<ExternalBookDetailDto?> GetBookByIsbnAsync(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return null;

            return await _provider.GetBookByIsbnAsync(isbn);
        }

        public async Task<(bool success, string? bookId, string? error)> ImportBookAsync(string externalId, string provider)
        {
            try
            {
                if (provider.ToLower() != "openlibrary")
                    return (false, null, "Provider not supported");

                // Busca detalhes do livro externo
                var externalBook = await _provider.GetBookByIdAsync(externalId);
                
                if (externalBook == null)
                    return (false, null, "Book not found in external provider");

                // Verifica se já existe no banco
                var existingBook = await _bookRepository.GetBookByNameAsync(externalBook.Title);
                if (existingBook != null)
                    return (false, existingBook.Id, "Book already exists in database");

                // Cria o livro no banco local
                var book = new Book
                {
                    Title = externalBook.Title,
                    Author = externalBook.Author,
                    Description = externalBook.Description,
                    CoverArtUrl = externalBook.CoverUrl,
                    Rating = null // Será definido pelos usuários
                };

                await _bookRepository.CreateBookAsync(book);

                _logger.LogInformation("Book imported successfully: {Title} by {Author}", book.Title, book.Author);
                
                return (true, book.Id, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing book from external provider");
                return (false, null, "Error importing book");
            }
        }
    }
}