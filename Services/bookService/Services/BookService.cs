using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookService.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<(bool success, string? error)> DeleteBookAsync(string id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null)
                return (false, "Book not found.");

            await _bookRepository.DeleteBookAsync(id);

            return (true, null);
        }

        public async Task<IEnumerable<GetDto>> GetAllBooksAsync()
        {
            return await _bookRepository.GetAllBooksAsync();
        }

        public async Task<GetDto?> GetBookByIdAsync(string id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null)
            {
                return null;
            }

            return new GetDto
            {
                Id = book.Id!,
                Title = book.Title,
                Description = book.Description,
                Author = book.Author,
                CoverArtUrl = book.CoverArtUrl,
                Rating = book.Rating
            };
        }

        public async Task<(bool success, string? error)> RegisterAsync(RegisterDto dto)
        {
            if (await _bookRepository.GetBookByNameAsync(dto.Title) != null)
                return (false, "Book already exists.");

            var book = new Book
            {
                Title = dto.Title,
                Description = dto.Description,
                Author = dto.Author,
                CoverArtUrl = dto.CoverArtUrl,
                Rating = dto.Rating
            };

            await _bookRepository.CreateBookAsync(book);
            return (true, null);
        }

        public async Task<(bool success, string? error)> UpdateBookAsync(string id, RegisterDto dto)
        {
            var existingBook = _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null)
                return (false, "Livro não encontrado.");

            var book = new Book
            {
                Id = id,
                Title = dto.Title,
                Description = dto.Description,
                Author = dto.Author,
                CoverArtUrl = dto.CoverArtUrl,
                Rating = dto.Rating
            };

            await _bookRepository.UpdateBookAsync(id, book);

            return (true, null);
        }
    }
}