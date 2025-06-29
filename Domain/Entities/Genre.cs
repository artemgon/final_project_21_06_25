using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Genre : INotifyPropertyChanged
    {
        private string _genreName = string.Empty;
        private string? _description;

        public int GenreId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string GenreName 
        { 
            get => _genreName;
            set
            {
                if (_genreName != value)
                {
                    _genreName = value;
                    OnPropertyChanged(nameof(GenreName));
                }
            }
        }
        
        [MaxLength(500)]
        public string? Description 
        { 
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }
        
        // Navigation properties
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
