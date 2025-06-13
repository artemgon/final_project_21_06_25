using BookLibrary.ViewModels.AuthorManagement; 
using BookLibrary.ViewModels.Base;
using BookLibrary.ViewModels.BookManagement;
using BookLibrary.ViewModels.Commands;
using BookLibrary.ViewModels.GenreManagement;   
using BookLibrary.ViewModels.WishlistManagement; 
using System.Windows.Input;


namespace BookLibrary.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // Commands for navigation
        public ICommand NavigateToBookListCommand { get; }
        public ICommand NavigateToAddBookCommand { get; }
        public ICommand NavigateToAuthorManagerCommand { get; }
        public ICommand NavigateToGenreManagerCommand { get; }
        public ICommand NavigateToWishlistManagerCommand { get; }

        // Private fields for injected ViewModels
        private readonly BookListViewModel _bookListViewModel;
        private readonly BookDetailViewModel _bookDetailViewModel;
        private readonly AuthorManagerViewModel _authorManagerViewModel;
        private readonly GenreManagerViewModel _genreManagerViewModel;
        private readonly WishlistManagerViewModel _wishlistManagerViewModel;


        // Constructor: All dependencies (other ViewModels) are injected here by the DI container
        public MainViewModel(
            BookListViewModel bookListViewModel,
            BookDetailViewModel bookDetailViewModel,
            AuthorManagerViewModel authorManagerViewModel,
            GenreManagerViewModel genreManagerViewModel,
            WishlistManagerViewModel wishlistManagerViewModel)
        {
            _bookListViewModel = bookListViewModel;
            _bookDetailViewModel = bookDetailViewModel;
            _authorManagerViewModel = authorManagerViewModel;
            _genreManagerViewModel = genreManagerViewModel;
            _wishlistManagerViewModel = wishlistManagerViewModel;

            // Initialize navigation commands
            NavigateToBookListCommand = new RelayCommand(() => CurrentViewModel = _bookListViewModel);
            NavigateToAddBookCommand = new RelayCommand(() =>
            {
                _bookDetailViewModel.LoadForNewBook(); // Prepare for new book entry
                CurrentViewModel = _bookDetailViewModel;
            });
            NavigateToAuthorManagerCommand = new RelayCommand(() => CurrentViewModel = _authorManagerViewModel);
            NavigateToGenreManagerCommand = new RelayCommand(() => CurrentViewModel = _genreManagerViewModel);
            NavigateToWishlistManagerCommand = new RelayCommand(() => CurrentViewModel = _wishlistManagerViewModel);

            // Set the initial view when the application starts
            CurrentViewModel = _bookListViewModel;
        }
    }
}