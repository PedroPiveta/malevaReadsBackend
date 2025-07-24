using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bookService.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBook(RegisterDto dto)
        {
            var (success, error) = await _bookService.RegisterAsync(dto);

            if (!success)
                return BadRequest(error);

            return Created();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _bookService.GetAllBooksAsync();

            if (books == null || !books.Any())
                return NotFound("No books found.");

            return Ok(books);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetBookById(string id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();

            return Ok(book);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBook(string id, RegisterDto dto)
        {
            var (success, error) = await _bookService.UpdateBookAsync(id, dto);

            if (!success)
                return BadRequest(error);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBook(string id)
        {
            var (success, error) = await _bookService.DeleteBookAsync(id);

            if (!success)
                return BadRequest(error);

            return NoContent();
        }
    }
}