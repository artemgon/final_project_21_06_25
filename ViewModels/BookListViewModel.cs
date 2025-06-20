// BookLibrary.ViewModels/BookManagement/BookListViewModel.cs
// Make sure these using statements are present at the top of your file.
using BookLibrary.ApplicationServices.Contracts; // Assuming IBookService, IWishlistService, IGenreService are here
using Domain.Entities; // For Book entity
using BookLibrary.ViewModels.Messages; // NEW: For navigation messages
using CommunityToolkit.Mvvm.ComponentModel; // Required for [ObservableProperty] and ObservableObject
using CommunityToolkit.Mvvm.Input;       // Required for [RelayCommand] and AsyncRelayCommand
using CommunityToolkit.Mvvm.Messaging; // NEW: For IMessenger and WeakReferenceMessenger.Default
using System.Collections.ObjectModel;
using System.Linq; // Required for LINQ methods like OrderBy, Where, FirstOrDefault, Any
using System.Threading.Tasks; // Required for Task, async/await
using System.Windows; // Required for MessageBox (for temporary feedback)
using System;
using ApplicationServices.Contracts;
using ViewModels; // For StringComparison

namespace BookLibrary.ViewModels.BookManagement // Ensure this namespace matches your folder structure
{
    // Mark BookListViewModel as 'partial' to allow CommunityToolkit.Mvvm source generation
    public partial class BookListViewModel : ViewModelBase
    {
        private readonly IBookService _bookService;
        private readonly IWishlistService _wishlistService;
        private readonly IGenreService _genreService; // Needed for genre filter dropdown
        // private readonly MainViewModel _mainViewModel; // REMOVED: No direct reference to MainViewModel

        // ObservableCollection to hold the books displayed in the DataGrid
        [ObservableProperty] private ObservableCollection<Book> books;

        // Property to hold the currently selected book in the DataGrid
        [ObservableProperty] private Book selectedBook;

        // --- Search and Filter Properties ---
        [ObservableProperty] private string searchTerm;

        // Property for the selected reading status filter in the ComboBox
        [ObservableProperty] private string selectedReadingStatusFilter;

        // Collection for the reading status filter dropdown. "All" option is added.
        // This can be static as it's common across all instances and doesn't change.
        public static ObservableCollection<string> ReadingStatuses { get; } = new ObservableCollection<string>
        {
            "All", "To Read", "Reading", "Finished", "Abandoned" // Added "Finished" and "Abandoned"
        };

        // Property for the selected genre filter in the ComboBox
        [ObservableProperty] private Genre selectedGenreFilter; // Use Genre object for binding

        // Collection for genre filter dropdown. "All" option will be added dynamically.
        [ObservableProperty] private ObservableCollection<Genre> availableGenres;

        // Property to control the visibility of the loading indicator
        [ObservableProperty] private bool isLoading;

        // Stores the currently active sort property, to re-apply after filtering
        private string currentSortProperty = nameof(Book.Title); // Default sort by Title
        private bool isAscending = true; // Track sort direction


        // --- Commands ---
        // LoadBooksCommand will now be marked with [RelayCommand]
        [RelayCommand] // Auto-generates public AsyncRelayCommand LoadBooksCommand { get; }
        private async Task LoadBooksAsync() { /* ... implementation ... */ }

        // These commands will now use the messenger for navigation instead of _mainViewModel
        [RelayCommand] // Auto-generates public RelayCommand AddBookCommand { get; }
        private void AddBook()
        {
            // Send a message to request navigation to add a new book
            WeakReferenceMessenger.Default.Send(new NavigateToAddBookMessage());
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteBook))] // Auto-generates public AsyncRelayCommand EditBookCommand { get; }
        private async Task EditBookAsync()
        {
            if (SelectedBook != null)
            {
                // Send a message to request navigation to edit a specific book
                WeakReferenceMessenger.Default.Send(new NavigateToEditBookMessage(SelectedBook.BookId));
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteBook))] // Auto-generates public AsyncRelayCommand DeleteBookCommand { get; }
        private async Task DeleteBookAsync() { /* ... implementation ... */ }

        [RelayCommand] // Auto-generates public AsyncRelayCommand SearchCommand { get; }
        private async Task SearchBooksAsync() { /* ... implementation ... */ }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteBook))] // Auto-generates public AsyncRelayCommand AddToWishlistCommand { get; }
        private async Task AddToWishlistAsync() { /* ... implementation ... */ }

        // Sorting Commands are typically direct RelayCommands
        [RelayCommand] // Auto-generates public RelayCommand SortByTitleCommand { get; }
        private void SortByTitle() => SortBooks(nameof(Book.Title));
        
        [RelayCommand] // Auto-generates public RelayCommand SortByAuthorCommand { get; }
        private void SortByAuthor() => SortBooks(nameof(Book.Authors));
        
        [RelayCommand] // Auto-generates public RelayCommand SortByReadingStatusCommand { get; }
        private void SortByReadingStatus() => SortBooks(nameof(Book.ReadingStatus));
        
        [RelayCommand] // Auto-generates public RelayCommand SortByRatingCommand { get; }
        private void SortByRating() => SortBooks(nameof(Book.Rating));
        
        [RelayCommand] // Auto-generates public RelayCommand SortByPageCountCommand { get; }
        private void SortByPageCount() => SortBooks(nameof(Book.PageCount));
        
        [RelayCommand] // Auto-generates public RelayCommand SortByPublicationYearCommand { get; }
        private void SortByPublicationYear() => SortBooks(nameof(Book.PublicationYear));


        // Constructor: Services are injected here by the DI container
        public BookListViewModel(
            IBookService bookService,
            IWishlistService wishlistService,
            IGenreService genreService) // REMOVED: MainViewModel from constructor
        {
            _bookService = bookService;
            _wishlistService = wishlistService;
            _genreService = genreService;
            // _mainViewModel = mainViewModel; // REMOVED: No longer needed

            Books = new ObservableCollection<Book>();
            AvailableGenres = new ObservableCollection<Genre>(); // Initialize genre collection

            // Set initial filter state
            SelectedReadingStatusFilter = "All"; // Default to show all statuses

            // --- IMPORTANT: Remove manual command initializations if [RelayCommand] is used on the methods ---
            // LoadBooksCommand = new AsyncRelayCommand(LoadBooksAsync); // REMOVE THIS LINE
            // AddBookCommand = new RelayCommand(() => _mainViewModel.NavigateToAddBookCommand.Execute(null)); // REMOVE THIS LINE
            // EditBookCommand = new AsyncRelayCommand(EditBookAsync, CanEditOrDeleteBook); // REMOVE THIS LINE
            // DeleteBookCommand = new AsyncRelayCommand(DeleteBookAsync, CanEditOrDeleteBook); // REMOVE THIS LINE
            // SearchCommand = new AsyncRelayCommand(SearchBooksAsync); // REMOVE THIS LINE
            // AddToWishlistCommand = new AsyncRelayCommand(AddToWishlistAsync, CanEditOrDeleteBook); // REMOVE THIS LINE

            // SortByTitleCommand = new RelayCommand(() => SortBooks(nameof(Book.Title))); // REMOVE THESE LINES
            // SortByAuthorCommand = new RelayCommand(() => SortBooks(nameof(Book.Authors)));
            // SortByReadingStatusCommand = new RelayCommand(() => SortBooks(nameof(Book.ReadingStatus)));
            // SortByRatingCommand = new RelayCommand(() => SortBooks(nameof(Book.Rating)));
            // SortByPageCountCommand = new RelayCommand(() => SortBooks(nameof(Book.PageCount)));
            // SortByPublicationYearCommand = new RelayCommand(() => SortBooks(nameof(Book.PublicationYear)));


            // Subscribe to SelectedBook property changes to update CanExecute state of buttons
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SelectedBook))
                {
                    // Call NotifyCanExecuteChanged for the commands that depend on SelectedBook
                    EditBookCommand.NotifyCanExecuteChanged();
                    DeleteBookCommand.NotifyCanExecuteChanged();
                    AddToWishlistCommand.NotifyCanExecuteChanged();
                }
            };

            // Initial data loads when ViewModel is created (fire and forget)
            _ = LoadBooksAsync(); // Load all books initially
            _ = LoadAvailableGenresAsync(); // Load genres for filter dropdown
        }

        // --- Predicates for Command Execution ---

        /// <summary>
        /// Determines if Edit, Delete, or Add to Wishlist commands can be executed.
        /// Requires a book to be selected.
        /// </summary>
        private bool CanEditOrDeleteBook()
        {
            return SelectedBook != null;
        }

        // --- Asynchronous Methods (command implementations) ---

        /// <summary>
        /// Loads all books from the database. This is used for initial load or full refresh.
        /// After loading, it applies the current search/filter/sort settings.
        /// </summary>
        // [RelayCommand] attribute already generates the public command and links it.
        // private async Task LoadBooksAsync() - Method is already here, no change needed in content
        // ... (rest of LoadBooksAsync implementation) ...


        /// <summary>
        /// Loads all available genres for the filter dropdown.
        /// </summary>
        private async Task LoadAvailableGenresAsync()
        {
            try
            {
                var genres = await _genreService.GetAllGenresAsync();
                AvailableGenres.Clear();
                // Add an "All Genres" option for filtering
                AvailableGenres.Add(new Genre
                    { GenreId = 0, GenreName = "All Genres" }); // Assuming GenreId 0 means "All"
                foreach (var genre in genres.OrderBy(g => g.GenreName, StringComparer.OrdinalIgnoreCase))
                {
                    AvailableGenres.Add(genre);
                }

                SelectedGenreFilter = AvailableGenres.FirstOrDefault(); // Select "All Genres" by default
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading genres for filter: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Performs search based on searchTerm and selectedReadingStatusFilter.
        /// This method is called by the SearchCommand.
        /// </summary>
        // [RelayCommand] attribute already generates the public command and links it.
        // private async Task SearchBooksAsync() - Method is already here, no change needed in content
        // ... (rest of SearchBooksAsync implementation) ...


        /// <summary>
        /// Navigates to the BookDetailView for editing the selected book.
        /// </summary>
        // This method's content is now handled by the new EditBookAsync method above, which sends a message.
        // The original private async Task EditBookAsync() will be replaced by the public one with [RelayCommand]
        // This method is now handled by EditBookAsync() via message.
        // private async Task EditBookAsync() { ... } // REMOVE or replace with the new method that sends a message


        /// <summary>
        /// Deletes the currently selected book from the database.
        /// </summary>
        // [RelayCommand] attribute already generates the public command and links it.
        // private async Task DeleteBookAsync() - Method is already here, no change needed in content
        // ... (rest of DeleteBookAsync implementation) ...


        /// <summary>
        /// Adds the currently selected book to the wishlist.
        /// </summary>
        // [RelayCommand] attribute already generates the public command and links it.
        // private async Task AddToWishlistAsync() - Method is already here, no change needed in content
        // ... (rest of AddToWishlistAsync implementation) ...


        // --- Filtering and Sorting Logic ---

        /// <summary>
        /// Applies the current search term, reading status filter, and genre filter to the Books collection.
        /// This method is called internally after loading or when search/filter criteria change.
        /// </summary>
        private void ApplyFilterAndSort()
        {
            var filtered = Books.AsEnumerable(); // Start with the full loaded list

            // Apply Search Term filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                string lowerSearchTerm = SearchTerm.ToLower();
                filtered = filtered.Where(b =>
                    (b.Title != null && b.Title.ToLower().Contains(lowerSearchTerm)) ||
                    (b.Authors != null && b.Authors.Any() && b.Authors.Any(a =>
                        $"{a.FirstName} {a.LastName}".ToLower().Contains(lowerSearchTerm))) ||
                    (b.ISBN != null && b.ISBN.ToLower().Contains(lowerSearchTerm)) ||
                    (b.Summary != null && b.Summary.ToLower().Contains(lowerSearchTerm)));
            }

            // Apply Reading Status filter
            if (SelectedReadingStatusFilter != "All" && !string.IsNullOrWhiteSpace(SelectedReadingStatusFilter))
            {
                filtered = filtered.Where(b => b.ReadingStatus == SelectedReadingStatusFilter);
            }

            // Apply Genre filter
            if (SelectedGenreFilter != null && SelectedGenreFilter.GenreId != 0) // Assuming 0 means "All Genres"
            {
                filtered = filtered.Where(b => b.Genres.Any(g => g.GenreId == SelectedGenreFilter.GenreId));
            }

            // Apply current sort order to the filtered results
            IOrderedEnumerable<Book> sortedFiltered = ApplyCurrentSort(filtered);

            // Update the UI-bound collection
            Books.Clear();
            foreach (var book in sortedFiltered)
            {
                Books.Add(book);
            }
        }


        /// <summary>
        /// Sorts the Books collection based on the specified property name.
        /// Toggles sort direction if sorting by the same property again.
        /// </summary>
        /// <param name="propertyName">The name of the property to sort by.</param>
        /// <param name="toggleDirection">If true, toggles the sort direction if the same property is clicked again.</param>
        private void SortBooks(string propertyName, bool toggleDirection = true)
        {
            if (currentSortProperty == propertyName && toggleDirection)
            {
                isAscending = !isAscending; // Toggle direction if same column
            }
            else
            {
                currentSortProperty = propertyName;
                isAscending = true; // Default to ascending for new column
            }

            ApplyFilterAndSort(); // Re-apply filter and then sort with the new criteria
        }

        /// <summary>
        /// Applies the current sort order to an enumerable of Book.
        /// Used internally by ApplyFilterAndSort to ensure consistent sorting.
        /// </summary>
        private IOrderedEnumerable<Book> ApplyCurrentSort(IEnumerable<Book> source)
        {
            IOrderedEnumerable<Book> sorted;

            switch (currentSortProperty)
            {
                case nameof(Book.Title):
                    sorted = isAscending
                        ? source.OrderBy(b => b.Title, StringComparer.OrdinalIgnoreCase)
                        : source.OrderByDescending(b => b.Title, StringComparer.OrdinalIgnoreCase);
                    break;
                case nameof(Book.Authors):
                    // Sort by the first author's last name, then first name
                    sorted = isAscending
                        ? source.OrderBy(b => b.Authors.FirstOrDefault()?.LastName, StringComparer.OrdinalIgnoreCase)
                            .ThenBy(b => b.Authors.FirstOrDefault()?.FirstName, StringComparer.OrdinalIgnoreCase)
                        : source.OrderByDescending(b => b.Authors.FirstOrDefault()?.LastName,
                                StringComparer.OrdinalIgnoreCase)
                            .ThenByDescending(b => b.Authors.FirstOrDefault()?.FirstName,
                                StringComparer.OrdinalIgnoreCase);
                    break;
                case nameof(Book.ReadingStatus):
                    // Sort by reading status based on a defined order, then by title alphabetically
                    sorted = isAscending
                        ? source.OrderBy(b => GetReadingStatusOrder(b.ReadingStatus))
                            .ThenBy(b => b.Title, StringComparer.OrdinalIgnoreCase)
                        : source.OrderByDescending(b => GetReadingStatusOrder(b.ReadingStatus))
                            .ThenByDescending(b => b.Title, StringComparer.OrdinalIgnoreCase);
                    break;
                case nameof(Book.Rating):
                    sorted = isAscending
                        ? source.OrderBy(b => b.Rating ?? 0)
                        : source.OrderByDescending(b => b.Rating ?? 0); // Sort descending, nulls as 0
                    break;
                case nameof(Book.PageCount):
                    sorted = isAscending
                        ? source.OrderBy(b => b.PageCount)
                        : source.OrderByDescending(b => b.PageCount);
                    break;
                case nameof(Book.PublicationYear):
                    sorted = isAscending
                        ? source.OrderBy(b => b.PublicationYear)
                        : source.OrderByDescending(b => b.PublicationYear);
                    break;
                default:
                    // Default sort if property is not recognized (e.g., initial state)
                    sorted = source.OrderBy(b => b.Title, StringComparer.OrdinalIgnoreCase);
                    break;
            }

            return sorted;
        }

        // Helper to define explicit order for reading statuses
        private int GetReadingStatusOrder(string status)
        {
            return status?.ToLower() switch
            {
                "reading" => 1,
                "to read" => 2,
                "finished" => 3,
                "abandoned" => 4,
                _ => 5, // Default for "All" or unknown statuses
            };
        }
    }
}
