using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class WishlistItem
    {
        public int WishlistItemId { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; } 
        public string? Notes { get; set; }
        public string? ISBN { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
