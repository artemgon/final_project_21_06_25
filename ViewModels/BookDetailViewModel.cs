using BookLibrary.ApplicationServices.Contracts; // For IBookService
using Domain.Entities; // For Book entity
using BookLibrary.ViewModels.Base;

namespace BookLibrary.ViewModels.BookManagement
{
    public class BookDetailViewModel : ViewModelBase
    {
        private readonly IBookService _bookService;

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