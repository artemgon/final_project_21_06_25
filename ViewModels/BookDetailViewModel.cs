using BookLibrary.ApplicationServices.Contracts; // For IBookService
using Domain.Entities; // For Book entity
using Microsoft.Win32;
using System.Windows.Input;

namespace ViewModels
{
    public class BookDetailViewModel : ViewModelBase
    {
        private readonly IBookService _bookService;
        
        public ICommand SaveCommand { get; }
        public ICommand ChangeCoverImageCommand { get; }

        private Book _currentBook;
        public Book CurrentBook
        {
            get => _currentBook;
            set => SetProperty(ref _currentBook, value);
        }

        public BookDetailViewModel(IBookService bookService)
        {
            _bookService = bookService;
            CurrentBook = new Book(); // Initialize with an empty book for default state
            ChangeCoverImageCommand = new RelayCommand(ChangeCoverImage);
            SaveCommand = new RelayCommand(SaveBook, CanSaveBook);
        }
        
        private void ChangeCoverImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select Cover Image"
            };
            if (dialog.ShowDialog() == true)
            {
                CurrentBook.CoverImagePath = dialog.FileName;
                OnPropertyChanged(nameof(CurrentBook));
            }
        }
        
        private void SaveBook()
        {
            if (CurrentBook.BookId == 0)
                _bookService.CreateBook(CurrentBook, null, null); // Assuming authors and genres are handled separately
            else
                _bookService.UpdateBook(CurrentBook, null, null); // Assuming authors and genres are handled separately
            // Optionally notify UI or close dialog
        }

        private bool CanSaveBook()
        {
            // Add validation logic as needed
            return !string.IsNullOrWhiteSpace(CurrentBook.Title);
        }

        public void LoadForNewBook()
        {
            CurrentBook = new Book(); // Resets for new book entry
            // Clear any related selections (authors, genres) here later
        }

        public void LoadBook(int bookId)
        {
            CurrentBook = _bookService.GetBookDetails(bookId);
            // Load related data (authors, genres) here later
        }
        // Add Save/Cancel Commands, validation logic later
    }
}