using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace bookService.Data.Repositories
{


    public class BookRepository : IBookRepository
    {
        private readonly IMongoCollection<Book> _books;

        public BookRepository(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("books");
            _books = database.GetCollection<Book>("books");
        }
        public async Task<IEnumerable<GetDto>> GetAllBooksAsync()
        {
            var books = await _books.Find(book => true).ToListAsync();
            return books.Select(book => new GetDto
            {
                Id = book.Id!,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                CoverArtUrl = book.CoverArtUrl,
                Rating = book.Rating
            });
        }

        public async Task<Book?> GetBookByIdAsync(string id)
        {
            return await _books.Find(book => book.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateBookAsync(Book book)
        {
            await _books.InsertOneAsync(book);
        }

        public async Task UpdateBookAsync(string id, Book book)
        {
            await _books.ReplaceOneAsync(b => b.Id == id, book);
        }

        public async Task DeleteBookAsync(string id)
        {
            await _books.DeleteOneAsync(b => b.Id == id);
        }

        public async Task<Book?> GetBookByNameAsync(string name)
        {
            var book = await _books.Find(book => book.Title == name).FirstOrDefaultAsync();
            if (book == null)
            {
                return null;
            }
            return new Book
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                CoverArtUrl = book.CoverArtUrl,
                Rating = book.Rating
            };
        }
    }
}