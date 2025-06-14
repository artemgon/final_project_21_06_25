using BookLibrary.ApplicationServices.Contracts;
using Domain.Entities;
using System; 
using System.Collections.ObjectModel;
using System.Linq; 
using System.Windows; 
using System.Windows.Input;

namespace ViewModels
{
    public class BookListViewModel : ViewModelBase
    {
        private readonly IBookService _bookService;

        private ObservableCollection<Book> _books;
        public ObservableCollection<Book> Books
        {
            get => _books;
            set => SetProperty(ref _books, value);
        }

        private Book _selectedBook;
        public Book SelectedBook
        {
            get => _selectedBook;
            set
            {
                SetProperty(ref _selectedBook, value);
                // CORRECTED LINE:
                CommandManager.InvalidateRequerySuggested(); // This tells WPF to re-evaluate all commands' CanExecute status
            }
        }

        // Search and Filter Properties
        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        private string _selectedReadingStatusFilter;
        public string SelectedReadingStatusFilter
        {
            get => _selectedReadingStatusFilter;
            set => SetProperty(ref _selectedReadingStatusFilter, value);
        }

        private int? _selectedGenreFilterId;
        public int? SelectedGenreFilterId
        {
            get => _selectedGenreFilterId;
            set => SetProperty(ref _selectedGenreFilterId, value);
        }

        public ObservableCollection<string> ReadingStatuses { get; } = new ObservableCollection<string>
        {
            "All", "To Read", "Reading", "Read"
        };

        // Collection for genre filter dropdown (will be loaded dynamically)
        // private ObservableCollection<Genre> _availableGenres;
        // public ObservableCollection<Genre> AvailableGenres { get => _availableGenres; set => SetProperty(ref _availableGenres, value); }


        public ICommand LoadBooksCommand { get; }
        public ICommand AddBookCommand { get; }
        public ICommand EditBookCommand { get; }
        public ICommand DeleteBookCommand { get; }
        public ICommand SearchCommand { get; }

        public BookListViewModel(IBookService bookService/*, IGenreService genreService*/)
        {
            _bookService = bookService;
            // _genreService = genreService; // Uncomment when you implement genre loading

            Books = new ObservableCollection<Book>();

            LoadBooksCommand = new RelayCommand(LoadBooks);
            AddBookCommand = new RelayCommand(AddBook);
            EditBookCommand = new RelayCommand(EditBook, CanEditOrDeleteBook);
            DeleteBookCommand = new RelayCommand(DeleteBook, CanEditOrDeleteBook);
            SearchCommand = new RelayCommand(SearchBooks);

            LoadBooks();
            // LoadAvailableGenres(); // Uncomment and implement when genre service is ready
        }

        private void LoadBooks()
        {
            Books.Clear();
            var books = _bookService.GetAllBooksWithDetails();
            foreach (var book in books)
            {
                Books.Add(book);
            }
        }

        private void SearchBooks()
        {
            Books.Clear();
            var statusFilter = SelectedReadingStatusFilter == "All" ? null : SelectedReadingStatusFilter;
            var books = _bookService.SearchBooks(SearchTerm, statusFilter, SelectedGenreFilterId);
            foreach (var book in books)
            {
                Books.Add(book);
            }
        }

        private bool CanEditOrDeleteBook()
        {
            return SelectedBook != null; // Enable Edit/Delete only if a book is selected
        }

        private void AddBook()
        {

            MessageBox.Show("Add Book functionality will be implemented soon!");
            // ((App.Current.MainWindow.DataContext as MainViewModel)?.NavigateToAddBookCommand.Execute(null);
        }

        private void EditBook()
        {
            if (SelectedBook != null)
            {
                MessageBox.Show($"Edit Book functionality for: '{SelectedBook.Title}' will be implemented soon!");
            }
        }

        private void DeleteBook()
        {
            if (SelectedBook != null && MessageBox.Show($"Are you sure you want to delete '{SelectedBook.Title}'?", "Confirm Deletion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _bookService.DeleteBook(SelectedBook.BookId);
                    LoadBooks();
                    SelectedBook = null; 
                    MessageBox.Show("Book successfully deleted.", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}