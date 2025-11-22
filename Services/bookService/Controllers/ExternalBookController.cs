using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bookService.Controllers
{
    [ApiController]
    [Route("api/books/external")]
    public class ExternalBookController : ControllerBase
    {
        private readonly IExternalBookService _externalBookService;

        public ExternalBookController(IExternalBookService externalBookService)
        {
            _externalBookService = externalBookService;
        }

        /// <summary>
        /// Busca livros no Open Library
        /// </summary>
        /// <param name="query">Termo de busca (título, autor, ISBN, etc)</param>
        /// <returns>Lista de livros encontrados</returns>
        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchBooks([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required");

            var books = await _externalBookService.SearchBooksAsync(query);
            
            if (books == null || !books.Any())
                return NotFound("No books found");

            return Ok(books);
        }

        /// <summary>
        /// Obtém detalhes completos de um livro externo
        /// </summary>
        /// <param name="externalId">ID do livro no Open Library</param>
        /// <param name="provider">Provider (openlibrary)</param>
        /// <returns>Detalhes do livro</returns>
        [HttpGet("details/{externalId}")]
        [Authorize]
        public async Task<IActionResult> GetBookDetails(
            string externalId, 
            [FromQuery] string provider = "openlibrary")
        {
            var book = await _externalBookService.GetBookDetailsAsync(externalId, provider);
            
            if (book == null)
                return NotFound("Book not found");

            return Ok(book);
        }

        /// <summary>
        /// Busca livro por ISBN no Open Library
        /// </summary>
        /// <param name="isbn">ISBN do livro</param>
        /// <returns>Detalhes do livro</returns>
        [HttpGet("isbn/{isbn}")]
        [Authorize]
        public async Task<IActionResult> GetBookByIsbn(string isbn)
        {
            var book = await _externalBookService.GetBookByIsbnAsync(isbn);
            
            if (book == null)
                return NotFound("Book not found with this ISBN");

            return Ok(book);
        }

        /// <summary>
        /// Importa um livro do Open Library para o banco de dados local
        /// </summary>
        /// <param name="dto">Dados para importação (externalId e provider)</param>
        /// <returns>ID do livro importado</returns>
        [HttpPost("import")]
        [Authorize]
        public async Task<IActionResult> ImportBook([FromBody] ImportExternalBookDto dto)
        {
            var (success, bookId, error) = await _externalBookService.ImportBookAsync(
                dto.ExternalId, 
                dto.Provider);

            if (!success)
            {
                if (error == "Book already exists in database")
                    return Conflict(new { message = error, bookId });
                
                return BadRequest(error);
            }

            return Created($"/api/books/{bookId}", new { bookId, message = "Book imported successfully" });
        }
    }
}