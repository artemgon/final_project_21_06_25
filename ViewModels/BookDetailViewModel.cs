using BookLibrary.ApplicationServices.Contracts; 
using Domain.Entities;
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
            CurrentBook = new Book();
        }

        public void LoadForNewBook()
        {
            CurrentBook = new Book();
        }

        public void LoadBook(int bookId)
        {
            CurrentBook = _bookService.GetBookDetails(bookId);
        }

    }
}