using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookService.DTOs
{
    public class GetDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; } = null;
        public string Author { get; set; } = null!;
        public string? CoverArtUrl { get; set; } = null;
        public float? Rating { get; set; } = null;
    }
}