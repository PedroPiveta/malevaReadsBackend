using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookService.Data.Repositories
{
    public interface IBookRepository
    {   
        Task<IEnumerable<GetDto>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(string id);
        Task<Book?> GetBookByNameAsync(string name);
        Task CreateBookAsync(Book book);
        Task UpdateBookAsync(string id, Book book);
        Task DeleteBookAsync(string id);
    }
}