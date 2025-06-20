// BookLibrary.ViewModels/MainViewModel.cs
using BookLibrary.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows.Input; // Crucial for ICommand
using BookLibrary.ViewModels.BookManagement;
using BookLibrary.ViewModels.AuthorManagement;
using BookLibrary.ViewModels.GenreManagement;
using BookLibrary.ViewModels.WishlistManagement;
using System.Threading.Tasks;

namespace ViewModels
{
    public partial class MainViewModel : ObservableRecipient
    {
        private ObservableObject _currentViewModel;
        public ObservableObject CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // Commands for main navigation tabs
        public ICommand NavigateToBookListCommand { get; }
        public ICommand NavigateToAuthorManagerCommand { get; }
        public ICommand NavigateToGenreManagerCommand { get; }
        public ICommand NavigateToWishlistManagerCommand { get; }

        // --- RESTORE THESE PUBLIC COMMANDS FOR MAINWINDOW.XAML BINDING ---
        public ICommand NavigateToAddBookCommand { get; } // Re-added public property
        public ICommand NavigateToEditBookCommand { get; } // Re-added public property (though BookListViewModel still sends message for it)
        // Note: NavigateToEditBookCommand is primarily triggered by BookListViewModel sending a message,
        // but having it here won't hurt, especially if there's any other direct binding.
        // The BookListViewModel's EditBookAsync method now sends the message, so this one acts as a direct entry.


        // Private fields for the ViewModels that are managed by MainViewModel for navigation.
        // These are injected via the constructor.
        private readonly BookListViewModel _bookListViewModel;
        private readonly BookDetailViewModel _bookDetailViewModel;
        private readonly AuthorManagerViewModel _authorManagerViewModel;
        private readonly GenreManagerViewModel _genreManagerViewModel;
        private readonly WishlistManagerViewModel _wishlistManagerViewModel;


        // Constructor: All managed ViewModels are injected here by the DI container.
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

            // Initialize navigation commands for main tabs
            NavigateToBookListCommand = new RelayCommand(() => CurrentViewModel = _bookListViewModel);
            NavigateToAuthorManagerCommand = new RelayCommand(() => CurrentViewModel = _authorManagerViewModel);
            NavigateToGenreManagerCommand = new RelayCommand(() => CurrentViewModel = _genreManagerViewModel);
            NavigateToWishlistManagerCommand = new RelayCommand(() => CurrentViewModel = _wishlistManagerViewModel);

            // --- Initialize commands for Add/Edit Book navigation (for direct UI binding in MainWindow) ---
            NavigateToAddBookCommand = new AsyncRelayCommand(async () =>
            {
                await _bookDetailViewModel.LoadForNewBookAsync(); // Prepare for new book entry
                CurrentViewModel = _bookDetailViewModel; // Switch to the BookDetailView
            });

            NavigateToEditBookCommand = new AsyncRelayCommand<int>(async (bookId) =>
            {
                await _bookDetailViewModel.LoadBookAsync(bookId); // Load specific book for editing
                CurrentViewModel = _bookDetailViewModel; // Switch to the BookDetailView
            });


            // --- Subscribe to Navigation Messages ---
            // These subscriptions are for other ViewModels (like BookListViewModel) to request navigation.
            IsActive = true; // Activate the messenger for this ViewModel

            // Subscribe to "add new book" requests from other ViewModels
            WeakReferenceMessenger.Default.Register<NavigateToAddBookMessage>(this, async (recipient, message) =>
            {
                var mainVm = (MainViewModel)recipient;
                await mainVm._bookDetailViewModel.LoadForNewBookAsync();
                mainVm.CurrentViewModel = mainVm._bookDetailViewModel;
            });

            // Subscribe to "edit book" requests from other ViewModels, passing the book ID
            WeakReferenceMessenger.Default.Register<NavigateToEditBookMessage>(this, async (recipient, message) =>
            {
                var mainVm = (MainViewModel)recipient;
                await mainVm._bookDetailViewModel.LoadBookAsync(message.Value);
                mainVm.CurrentViewModel = mainVm._bookDetailViewModel;
            });

            // Set the initial view when the application starts
            CurrentViewModel = _bookListViewModel;
        }
    }
}
