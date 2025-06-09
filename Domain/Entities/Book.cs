using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public int? PublicationYear { get; set; }
        public string ISBN { get; set; }
        public int? PageCount { get; set; }
        public string Summary { get; set; }
        public string CoverImagePath { get; set; }
        public string ReadingStatus { get; set; }
        public int? Rating { get; set; } 
        public string Notes { get; set; }

        public List<Author> Authors { get; set; } = [];
        public List<Genre> Genres { get; set; } = [];
    }
}
