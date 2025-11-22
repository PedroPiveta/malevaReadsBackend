using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookService.DTOs
{
    public class ImportExternalBookDto
    {
        public string ExternalId { get; set; } = null!;
        public string Provider { get; set; } = null!;        
    }
}