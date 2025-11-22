using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookService.DTOs
{
    public class ExternalBookSearchDto
    {
        public string ExternalId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public int? FirstPublishYear { get; set; }
        public string Provider { get; set; } = null!;
        public int? EditionCount { get; set; }
        public List<string>? Languages { get; set; }
    }
}