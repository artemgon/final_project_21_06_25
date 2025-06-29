using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using BookLibrary.Domain.Entities;
using Domain.Enums;
using System.Runtime.CompilerServices;

namespace Domain.Entities
{
    public class Book : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private int? _publicationYear;
        private string? _isbn;
        private int? _pageCount;
        private string? _summary;
        private string? _coverImagePath;
        private ReadingStatus? _readingStatus;
        private int? _rating;
        private string? _notes;

        public int BookId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title 
        { 
            get => _title;
            set => SetProperty(ref _title, value);
        }
        
        public int? PublicationYear 
        { 
            get => _publicationYear;
            set => SetProperty(ref _publicationYear, value);
        }
        
        [MaxLength(20)]
        public string? ISBN 
        { 
            get => _isbn;
            set => SetProperty(ref _isbn, value);
        }
        
        public int? PageCount 
        { 
            get => _pageCount;
            set => SetProperty(ref _pageCount, value);
        }
        
        public string? Summary 
        { 
            get => _summary;
            set => SetProperty(ref _summary, value);
        }
        
        [MaxLength(500)]
        public string? CoverImagePath 
        { 
            get => _coverImagePath;
            set => SetProperty(ref _coverImagePath, value);
        }
        
        public ReadingStatus? ReadingStatus 
        { 
            get => _readingStatus;
            set => SetProperty(ref _readingStatus, value);
        }
        
        [Range(1, 5)]
        public int? Rating 
        { 
            get => _rating;
            set => SetProperty(ref _rating, value);
        }
        
        public string? Notes 
        { 
            get => _notes;
            set => SetProperty(ref _notes, value);
        }
        
        // Navigation properties
        public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
        public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
