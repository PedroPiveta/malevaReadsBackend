using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookService.DTOs
{
    public class ExternalBookDetailDto
    {
        public string ExternalId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public string? PublishYear { get; set; }
        public int? PageCount { get; set; }
        public string Provider { get; set; } = null!;
    }
}