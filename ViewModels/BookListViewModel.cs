// BookLibrary.ViewModels/BookManagement/BookListViewModel.cs
// Make sure these using statements are present at the top of your file.
using BookLibrary.ApplicationServices.Contracts; // Assuming IBookService, IWishlistService, IGenreService are here
using Domain.Entities; // For Book entity
using Domain.Enums; // Add this for ReadingStatus enum
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

namespace ViewModels // Fixed namespace to match file location
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
        private async Task LoadBooksAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadBooksAsync: Starting to load books from database...");
                
                // Set loading state on UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                });

                // Execute database query
                var allBooks = await _bookService.GetAllBooksWithDetailsAsync();
                
                System.Diagnostics.Debug.WriteLine($"LoadBooksAsync: Retrieved {allBooks?.Count() ?? 0} books from service");
                
                // Update UI on the UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Books.Clear();
                    
                    if (allBooks != null)
                    {
                        foreach (var book in allBooks.OrderBy(b => b.Title))
                        {
                            Books.Add(book);
                            System.Diagnostics.Debug.WriteLine($"Added book to UI: {book.Title} (ID: {book.BookId})");
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"LoadBooksAsync: UI updated. Final count in collection: {Books.Count}");
                    
                    // Force UI binding refresh
                    OnPropertyChanged(nameof(Books));
                });
                
                // Apply filters after loading
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (Books.Count > 0)
                    {
                        ApplyFilterAndSort(); // Apply current filters after loading
                        System.Diagnostics.Debug.WriteLine($"LoadBooksAsync: After applying filters, count: {Books.Count}");
                    }
                });
                
                System.Diagnostics.Debug.WriteLine($"LoadBooksAsync: Completed successfully. Total books in UI: {Books.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadBooksAsync error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"LoadBooksAsync stack trace: {ex.StackTrace}");
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Error loading books: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                });
            }
        }

        // Refresh command - same as LoadBooks but with user feedback
        [RelayCommand]
        private async Task RefreshBooksAsync()
        {
            await LoadBooksAsync();
            MessageBox.Show("Books refreshed successfully!", "Refresh Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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
        private async Task DeleteBookAsync()
        {
            if (SelectedBook == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedBook.Title}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _bookService.DeleteBookAsync(SelectedBook.BookId);
                    Books.Remove(SelectedBook);
                    SelectedBook = null;
                    
                    // Refresh the list to get updated data and potentially renumbered IDs
                    await LoadBooksAsync();
                    
                    MessageBox.Show("Book deleted successfully!", "Delete Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        [RelayCommand] // Auto-generates public AsyncRelayCommand SearchCommand { get; }
        private async Task SearchBooksAsync()
        {
            IsLoading = true;
            try
            {
                ApplyFilterAndSort(); // Re-apply filters with current search criteria
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching books: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteBook))] // Auto-generates public AsyncRelayCommand AddToWishlistCommand { get; }
        private async Task AddToWishlistAsync()
        {
            if (SelectedBook == null)
                return;

            IsLoading = true;
            try
            {
                // Check if book is already in wishlist by comparing title and author
                var existingWishlistItems = await _wishlistService.GetAllWishlistItemsAsync();
                if (existingWishlistItems.Any(w => w.Title == SelectedBook.Title && 
                    w.Author == (SelectedBook.Authors?.FirstOrDefault()?.FirstName + " " + SelectedBook.Authors?.FirstOrDefault()?.LastName)?.Trim()))
                {
                    MessageBox.Show("This book is already in your wishlist!", "Already in Wishlist", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Create a new wishlist item from the selected book
                var wishlistItem = new WishlistItem
                {
                    Title = SelectedBook.Title,
                    Author = SelectedBook.Authors?.FirstOrDefault() != null 
                        ? $"{SelectedBook.Authors.FirstOrDefault().FirstName} {SelectedBook.Authors.FirstOrDefault().LastName}".Trim()
                        : "Unknown Author",
                    ISBN = SelectedBook.ISBN,
                    DateAdded = DateTime.Now,
                    Notes = "" // Default empty notes
                };

                await _wishlistService.AddWishlistItemAsync(wishlistItem);
                MessageBox.Show($"'{SelectedBook.Title}' added to wishlist!", "Added to Wishlist", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding book to wishlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Add missing sort commands that are referenced in the XAML
        [RelayCommand]
        private void SortByTitle()
        {
            SortBooks(nameof(Book.Title));
        }

        [RelayCommand]
        private void SortByAuthor()
        {
            SortBooks(nameof(Book.Authors));
        }

        [RelayCommand]
        private void SortByReadingStatus()
        {
            SortBooks(nameof(Book.ReadingStatus));
        }

        [RelayCommand]
        private void SortByRating()
        {
            SortBooks(nameof(Book.Rating));
        }

        [RelayCommand]
        private void SortByPageCount()
        {
            SortBooks(nameof(Book.PageCount));
        }

        [RelayCommand]
        private void SortByPublicationYear()
        {
            SortBooks(nameof(Book.PublicationYear));
        }


        // Constructor: Services are injected here by the DI container
        public BookListViewModel(
            IBookService bookService,
            IWishlistService wishlistService,
            IGenreService genreService) // REMOVED: MainViewModel from constructor
        {
            _bookService = bookService;
            _wishlistService = wishlistService;
            _genreService = genreService;

            Books = new ObservableCollection<Book>();
            AvailableGenres = new ObservableCollection<Genre>(); // Initialize genre collection

            // Set initial filter state
            SelectedReadingStatusFilter = "All"; // Default to show all statuses

            // Initialize the search term and genre filter to avoid null issues
            SearchTerm = string.Empty;

            // Force initial data load when ViewModel is created, using the same pattern as AuthorManagerViewModel
            System.Diagnostics.Debug.WriteLine("BookListViewModel constructor: Starting initial load");
            _ = InitializeAsync();
        }

        // Override the OnPropertyChanged method to handle SelectedBook changes specifically
        partial void OnSelectedBookChanged(Book? value)
        {
            System.Diagnostics.Debug.WriteLine($"OnSelectedBookChanged: New value = {value?.Title ?? "null"}");
            
            // Explicitly notify that the CanExecute state of commands should be re-evaluated
            EditBookCommand.NotifyCanExecuteChanged();
            DeleteBookCommand.NotifyCanExecuteChanged();
            AddToWishlistCommand.NotifyCanExecuteChanged();
            
            System.Diagnostics.Debug.WriteLine($"OnSelectedBookChanged: Commands notified for book: {value?.Title ?? "null"}");
        }

        // Separate initialization method to handle async loading properly - matching AuthorManagerViewModel pattern
        private async Task InitializeAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("BookListViewModel InitializeAsync: Loading books and genres...");
                
                // Load genres first, then books - same order as AuthorManagerViewModel
                await LoadAvailableGenresAsync(); // Load genres for filter dropdown
                await LoadBooksAsync(); // Load all books initially
                
                System.Diagnostics.Debug.WriteLine($"BookListViewModel InitializeAsync: Loaded {Books.Count} books and {AvailableGenres.Count} genres");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BookListViewModel InitializeAsync error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"BookListViewModel InitializeAsync stack trace: {ex.StackTrace}");
                
                // Show error on UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Error loading initial data: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
        }

        // --- Predicates for Command Execution ---

        /// <summary>
        /// Determines if Edit, Delete, or Add to Wishlist commands can be executed.
        /// Requires a book to be selected.
        /// </summary>
        private bool CanEditOrDeleteBook()
        {
            bool canExecute = SelectedBook != null;
            System.Diagnostics.Debug.WriteLine($"CanEditOrDeleteBook: SelectedBook is {(SelectedBook != null ? $"'{SelectedBook.Title}'" : "null")}, returning {canExecute}");
            return canExecute;
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
            System.Diagnostics.Debug.WriteLine($"ApplyFilterAndSort: Starting with {Books.Count} books");
            System.Diagnostics.Debug.WriteLine($"ApplyFilterAndSort: SearchTerm='{SearchTerm}', SelectedReadingStatusFilter='{SelectedReadingStatusFilter}', SelectedGenreFilter={SelectedGenreFilter?.GenreName}");
            
            var filtered = Books.AsEnumerable(); // Start with the full loaded list

            // Apply Search Term filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                string lowerSearchTerm = SearchTerm.ToLower();
                System.Diagnostics.Debug.WriteLine($"ApplyFilterAndSort: Applying search term filter: '{lowerSearchTerm}'");
                
                filtered = filtered.Where(b =>
                    (b.Title != null && b.Title.ToLower().Contains(lowerSearchTerm)) ||
                    (b.Authors != null && b.Authors.Any() && b.Authors.Any(a =>
                        $"{a.FirstName} {a.LastName}".ToLower().Contains(lowerSearchTerm))) ||
                    (b.ISBN != null && b.ISBN.ToLower().Contains(lowerSearchTerm)) ||
                    (b.Summary != null && b.Summary.ToLower().Contains(lowerSearchTerm)));
            }

            // Apply Reading Status filter - fix the logic here
            if (!string.IsNullOrWhiteSpace(SelectedReadingStatusFilter) && 
                SelectedReadingStatusFilter != "All")
            {
                System.Diagnostics.Debug.WriteLine($"ApplyFilterAndSort: Applying reading status filter: '{SelectedReadingStatusFilter}'");
                
                // Map the UI string values to the actual enum values
                ReadingStatus? targetStatus = SelectedReadingStatusFilter switch
                {
                    "To Read" => ReadingStatus.NotStarted,
                    "Reading" => ReadingStatus.InProgress,
                    "Finished" => ReadingStatus.Completed,
                    "Abandoned" => ReadingStatus.Dropped,
                    _ => null
                };
                
                if (targetStatus.HasValue)
                {
                    filtered = filtered.Where(b => b.ReadingStatus == targetStatus.Value);
                    System.Diagnostics.Debug.WriteLine($"ApplyFilterAndSort: Filtering by ReadingStatus: {targetStatus.Value}");
                }
            }

            // Apply Genre filter - ensure we handle null properly
            if (SelectedGenreFilter != null && SelectedGenreFilter.GenreId != 0) // Assuming 0 means "All Genres"
            {
                System.Diagnostics.Debug.WriteLine($"ApplyFilterAndSort: Applying genre filter: '{SelectedGenreFilter.GenreName}'");
                filtered = filtered.Where(b => b.Genres != null && b.Genres.Any(g => g.GenreId == SelectedGenreFilter.GenreId));
            }

            System.Diagnostics.Debug.WriteLine($"ApplyFilterAndSort: After filtering, found {filtered.Count()} books");

            // Apply current sort order to the filtered results
            IOrderedEnumerable<Book> sortedFiltered = ApplyCurrentSort(filtered);

            // Update the UI-bound collection - but don't clear and re-add if it's the same
            var sortedList = sortedFiltered.ToList();
            System.Diagnostics.Debug.WriteLine($"ApplyFilterAndSort: Final sorted list has {sortedList.Count} books");
            
            // Only update if the collection actually changed
            Books.Clear();
            foreach (var book in sortedList)
            {
                Books.Add(book);
                System.Diagnostics.Debug.WriteLine($"ApplyFilterAndSort: Added book to final collection: {book.Title}");
            }
            
            System.Diagnostics.Debug.WriteLine($"ApplyFilterAndSort: Completed. Final Books.Count = {Books.Count}");
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
        private int GetReadingStatusOrder(ReadingStatus? status)
        {
            return status switch
            {
                ReadingStatus.InProgress => 1,
                ReadingStatus.NotStarted => 2,
                ReadingStatus.Completed => 3,
                ReadingStatus.OnHold => 4,
                ReadingStatus.Dropped => 5,
                null => 6, // Default for null values
                _ => 7, // Default for any other values
            };
        }

        // Add a debug method to test book-author relationships
        public async Task<string> TestBookAuthorRelationshipsAsync()
        {
            try
            {
                var books = await _bookService.GetAllBooksWithDetailsAsync();
                var result = $"Found {books.Count()} books:\n\n";
                
                foreach (var book in books)
                {
                    result += $"Book: {book.Title} (ID: {book.BookId})\n";
                    if (book.Authors != null && book.Authors.Any())
                    {
                        result += $"  Authors: {string.Join(", ", book.Authors.Select(a => $"{a.FirstName} {a.LastName}"))}\n";
                    }
                    else
                    {
                        result += "  Authors: None found (this is why you see N/A)\n";
                    }
                    result += "\n";
                }
                
                return result;
            }
            catch (Exception ex)
            {
                return $"Error testing relationships: {ex.Message}";
            }
        }
    }
}
