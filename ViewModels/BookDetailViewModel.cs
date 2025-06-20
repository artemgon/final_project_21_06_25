// BookLibrary.ViewModels/BookManagement/BookDetailViewModel.cs
// Make sure these using statements are present at the top of your file.
using BookLibrary.ApplicationServices.Contracts; // For IBookService, IAuthorService, IGenreService
using Domain.Entities; // For Book, Author, Genre entities// For ViewModelBase (which should inherit ObservableObject)
using CommunityToolkit.Mvvm.ComponentModel; // Required for [ObservableProperty] and ObservableObject
using CommunityToolkit.Mvvm.Input;       // Required for [RelayCommand] and AsyncRelayCommand
using Microsoft.Win32; // For OpenFileDialog
using System.Collections.ObjectModel; // For ObservableCollection
using System.Linq; // For LINQ methods like Any(), OrderBy(), Where()
using System.Threading.Tasks; // Required for Task, async/await
using System.Windows; // Required for MessageBox
using System;
using ApplicationServices.Contracts;
using BookLibrary.Domain.Entities;
using Domain.Entities;
using ViewModels;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand; // For DateTime, StringComparison

namespace BookLibrary.ViewModels.BookManagement // Ensure this namespace matches your folder structure
{
    // Mark BookDetailViewModel as 'partial' for CommunityToolkit.Mvvm source generation
    public partial class BookDetailViewModel : ViewModelBase
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IGenreService _genreService;

        // Represents the book currently being edited or created.
        [ObservableProperty]
        private Book currentBook;

        // Flag to indicate if we are in "new book" mode or "edit book" mode.
        [ObservableProperty]
        private bool isNewBook;

        // Property to control the visibility of the loading indicator.
        [ObservableProperty]
        private bool isLoading;

        // --- Collections for Author/Genre Selection UI ---

        // Holds all authors available in the database, wrapped in SelectableAuthorViewModel.
        public ObservableCollection<SelectableAuthorViewModel> AllAvailableAuthors { get; } = new ObservableCollection<SelectableAuthorViewModel>();
        // Holds all genres available in the database, wrapped in SelectableGenreViewModel.
        public ObservableCollection<SelectableGenreViewModel> AllAvailableGenres { get; } = new ObservableCollection<SelectableGenreViewModel>();

        // Properties for the search boxes within author/genre selection ListBoxes.
        [ObservableProperty]
        private string authorSearchTerm;
        [ObservableProperty]
        private string genreSearchTerm;

        // Filtered views of available authors/genres. These are bound directly to the UI ListBoxes.
        public ObservableCollection<SelectableAuthorViewModel> FilteredAvailableAuthors { get; } = new ObservableCollection<SelectableAuthorViewModel>();
        public ObservableCollection<SelectableGenreViewModel> FilteredAvailableGenres { get; } = new ObservableCollection<SelectableGenreViewModel>();

        // --- Commands ---
        // Command to load a specific book for editing (takes bookId as parameter).
        public AsyncRelayCommand<int> LoadBookCommand { get; }
        // Command to prepare the ViewModel for creating a new book.
        public AsyncRelayCommand LoadForNewBookCommand { get; }
        // Command to save (add or update) the current book. Includes a CanExecute predicate.
        public AsyncRelayCommand SaveBookCommand { get; }
        // Command to open a file dialog for selecting a cover image.
        public RelayCommand ChangeCoverImageCommand { get; }
        // Command to cancel the operation and typically navigate back.
        public RelayCommand CancelCommand { get; }


        // Constructor: Services are injected here by the Dependency Injection container.
        public BookDetailViewModel(IBookService bookService, IAuthorService authorService, IGenreService genreService)
        {
            _bookService = bookService;
            _authorService = authorService;
            _genreService = genreService;

            // Initialize CurrentBook with a default empty instance.
            // This ensures CurrentBook is never null and can be bound to immediately.
            currentBook = new Book
            {
                Authors = new List<Author>(), // Initialize collections to avoid NREs
                Genres = new List<Genre>()
            };

            // --- Initialize Commands ---
            // Note: If you previously had `public ICommand SomeCommand { get; }`
            // and `SomeCommand = new RelayCommand(...)` inside the constructor,
            // and then also used `[RelayCommand]` on the method, you need to
            // remove the manual `public ICommand SomeCommand { get; }` declaration.
            // However, for commands that take parameters or are linked from other ViewModels,
            // explicit initialization in the constructor like these are correct.
            LoadBookCommand = new AsyncRelayCommand<int>(LoadBookAsync);
            LoadForNewBookCommand = new AsyncRelayCommand(LoadForNewBookAsync);
            SaveBookCommand = new AsyncRelayCommand(SaveBookAsync, CanSaveBook); // Predicate for validation
            ChangeCoverImageCommand = new RelayCommand(ChangeCoverImage);
            CancelCommand = new RelayCommand(Cancel);

            // Subscribe to search term changes for dynamic filtering of authors/genres.
            // The PropertyChanged event is inherited from ObservableObject (via ViewModelBase).
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AuthorSearchTerm))
                {
                    FilterAuthors();
                }
                if (e.PropertyName == nameof(GenreSearchTerm))
                {
                    FilterGenres();
                }
                // When CurrentBook or its properties change, re-evaluate SaveBookCommand
                // This is important because CanSaveBook depends on CurrentBook.Title, Year, etc.
                if (e.PropertyName == nameof(CurrentBook) || (CurrentBook != null &&
                    (e.PropertyName == nameof(CurrentBook.Title) ||
                     e.PropertyName == nameof(CurrentBook.PublicationYear) ||
                     e.PropertyName == nameof(CurrentBook.PageCount))))
                {
                    SaveBookCommand.NotifyCanExecuteChanged();
                }
            };
        }

        // --- Command Implementations ---

        /// <summary>
        /// Loads an existing book's details into CurrentBook for editing.
        /// Fetches the book with its associated authors and genres, then populates selectable lists.
        /// </summary>
        /// <param name="bookId">The ID of the book to load.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task LoadBookAsync(int bookId) // Changed from private to public
        {
            IsLoading = true; // Show loading indicator
            IsNewBook = false; // Mark as editing existing book
            CurrentBook = null; // Clear previous book data from UI

            try
            {
                CurrentBook = await _bookService.GetBookDetailsAsync(bookId); // Fetch book with details

                if (CurrentBook == null)
                {
                    MessageBox.Show($"Book with ID {bookId} not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // If book not found, reset to a new book state or navigate back
                    await LoadForNewBookAsync();
                    return;
                }

                // Load all authors and genres from DB (for selection UI)
                await LoadAllAuthorsAndGenresAsync();
                // Set which authors/genres are currently associated with this book
                SetSelectedAuthorsAndGenres();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                await LoadForNewBookAsync(); // Reset on error
            }
            finally
            {
                IsLoading = false; // Hide loading indicator
                SaveBookCommand.NotifyCanExecuteChanged(); // Re-evaluate save button state
            }
        }

        /// <summary>
        /// Prepares the ViewModel for adding a new book.
        /// Initializes CurrentBook with default values and loads all available authors/genres.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task LoadForNewBookAsync() // Changed from private to public
        {
            IsLoading = true; // Show loading indicator
            IsNewBook = true; // Mark as creating new book

            // Initialize CurrentBook with default empty values for a fresh form
            CurrentBook = new Book
            {
                Title = string.Empty,
                ISBN = string.Empty,
                PublicationYear = DateTime.Now.Year, // Default to current year
                PageCount = 0,
                ReadingStatus = "To Read", // Default status as per request
                Rating = 0, // Default rating
                Summary = string.Empty,
                CoverImagePath = string.Empty,
                Authors = new List<Author>(), // Initialize empty collections for a new book
                Genres = new List<Genre>()
            };

            try
            {
                await LoadAllAuthorsAndGenresAsync(); // Load all authors/genres for selection
                // No need to call SetSelectedAuthorsAndGenres for a new book; none are selected by default.
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error preparing for new book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false; // Hide loading indicator
                SaveBookCommand.NotifyCanExecuteChanged(); // Re-evaluate save button state
            }
        }

        /// <summary>
        /// Opens a file dialog to select a cover image and updates CurrentBook.CoverImagePath.
        /// </summary>
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
                // No need for OnPropertyChanged(nameof(CurrentBook)); if CurrentBook is [ObservableProperty]
                // Its properties changing will trigger UI updates if bound correctly.
            }
        }

        /// <summary>
        /// Determines if the SaveBookCommand can be executed.
        /// Performs basic validation on essential book properties.
        /// </summary>
        /// <returns>True if the book can be saved, false otherwise.</returns>
        private bool CanSaveBook()
        {
            // Must have a CurrentBook instance
            if (CurrentBook == null)
                return false;

            // Title cannot be empty
            if (string.IsNullOrWhiteSpace(CurrentBook.Title))
                return false;

            // PublicationYear must be a valid positive year and not in the future
            if (CurrentBook.PublicationYear <= 0 || CurrentBook.PublicationYear > DateTime.Now.Year)
                return false;

            // PageCount must be positive
            if (CurrentBook.PageCount <= 0)
                return false;

            // Rating must be between 1 and 5 (inclusive) if provided
            if (CurrentBook.Rating.HasValue && (CurrentBook.Rating.Value < 1 || CurrentBook.Rating.Value > 5))
                return false;
            
            // Optionally: Add more validation here (e.g., ISBN format, at least one author/genre, etc.)

            return true; // All validation checks passed
        }

        /// <summary>
        /// Saves (adds or updates) the current book to the database asynchronously.
        /// Updates the book's associated authors and genres based on UI selection.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task SaveBookAsync()
        {
            if (!CanSaveBook()) // Re-check validation
            {
                MessageBox.Show("Please fill in all required fields and ensure data is valid.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true; // Show loading indicator
            try
            {
                // Update CurrentBook's Authors and Genres collections based on what's selected in the UI
                CurrentBook.Authors.Clear();
                foreach (var selectableAuthor in AllAvailableAuthors.Where(a => a.IsSelected))
                {
                    CurrentBook.Authors.Add(selectableAuthor.Author);
                }

                CurrentBook.Genres.Clear();
                foreach (var selectableGenre in AllAvailableGenres.Where(g => g.IsSelected))
                {
                    CurrentBook.Genres.Add(selectableGenre.Genre);
                }

                if (IsNewBook)
                {
                    await _bookService.AddBookAsync(CurrentBook);
                    MessageBox.Show("Book added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _bookService.UpdateBookAsync(CurrentBook); // Assuming UpdateBookAsync exists in IBookService
                    MessageBox.Show("Book updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                // After saving, typically navigate back to the book list or close the form
                Cancel(); // Use Cancel to trigger navigation/form reset
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false; // Hide loading indicator
            }
        }

        /// <summary>
        /// Cancels the current operation and navigates back to the main book list.
        /// (In a real app, this would use a Messenger/EventAggregator for navigation).
        /// </summary>
        private void Cancel()
        {
            // This is a placeholder for actual navigation.
            // In a full MVVM framework (like using an EventAggregator or Messenger service),
            // this would send a message to MainViewModel to change the current view.
            MessageBox.Show("Operation cancelled. Returning to Book List.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // To actually navigate back, you would likely do something like:
            // WeakReferenceMessenger.Default.Send(new NavigateMessage(typeof(BookListViewModel)));
            // For now, assume MainViewModel handles the return implicitly or you'll manually navigate in testing.
        }

        // --- Helper Methods for Author/Genre Selection Logic ---

        /// <summary>
        /// Helper method to fetch all available authors and genres from the database
        /// and populate the AllAvailableAuthors and AllAvailableGenres collections.
        /// These collections are used by the UI's ListBoxes for selection.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task LoadAllAuthorsAndGenresAsync()
        {
            AllAvailableAuthors.Clear();
            FilteredAvailableAuthors.Clear(); // Clear filtered list too
            var authors = await _authorService.GetAllAuthorsAsync();
            foreach (var author in authors.OrderBy(a => a.LastName).ThenBy(a => a.FirstName)) // Order for display
            {
                var selectableAuthor = new SelectableAuthorViewModel(author);
                AllAvailableAuthors.Add(selectableAuthor);
                FilteredAvailableAuthors.Add(selectableAuthor); // Initially, all are in the filtered list
            }
        }

        /// <summary>
        /// Helper method to iterate through all available authors and genres
        /// and mark them as selected if they are associated with the CurrentBook.
        /// This is used when loading an existing book for editing.
        /// </summary>
        private void SetSelectedAuthorsAndGenres()
        {
            // Ensure CurrentBook's Authors and Genres are initialized
            // This is vital if the loaded book didn't have any associations
            if (CurrentBook.Authors == null) CurrentBook.Authors = new List<Author>();
            if (CurrentBook.Genres == null) CurrentBook.Genres = new List<Genre>();

            // Mark authors as selected based on CurrentBook's authors
            foreach (var selectableAuthor in AllAvailableAuthors)
            {
                selectableAuthor.IsSelected = CurrentBook.Authors.Any(a => a.AuthorId == selectableAuthor.Author.AuthorId);
            }

            // Mark genres as selected based on CurrentBook's genres
            foreach (var selectableGenre in AllAvailableGenres)
            {
                selectableGenre.IsSelected = CurrentBook.Genres.Any(g => g.GenreId == selectableGenre.Genre.GenreId);
            }

            // Re-apply filters in case the search term was already present
            FilterAuthors();
            FilterGenres();
        }

        /// <summary>
        /// Filters the AllAvailableAuthors collection based on AuthorSearchTerm.
        /// </summary>
        private void FilterAuthors()
        {
            FilteredAvailableAuthors.Clear();
            foreach (var authorVm in AllAvailableAuthors.Where(a =>
                string.IsNullOrEmpty(AuthorSearchTerm) ||
                a.FullName.Contains(AuthorSearchTerm, StringComparison.OrdinalIgnoreCase)))
            {
                FilteredAvailableAuthors.Add(authorVm);
            }
        }

        /// <summary>
        /// Filters the AllAvailableGenres collection based on GenreSearchTerm.
        /// </summary>
        private void FilterGenres()
        {
            FilteredAvailableGenres.Clear();
            foreach (var genreVm in AllAvailableGenres.Where(g =>
                string.IsNullOrEmpty(GenreSearchTerm) ||
                g.Genre.GenreName.Contains(GenreSearchTerm, StringComparison.OrdinalIgnoreCase)))
            {
                FilteredAvailableGenres.Add(genreVm);
            }
        }
    }

    // --- Helper ViewModels for Selection ---

    /// <summary>
    /// ViewModel wrapper for an Author, used in selection UIs (e.g., ListBox with CheckBoxes).
    /// </summary>
    public partial class SelectableAuthorViewModel : ObservableObject
    {
        public Author Author { get; }

        [ObservableProperty]
        private bool isSelected; // Binds to the CheckBox

        public string FullName => $"{Author.FirstName} {Author.LastName}"; // Display property

        public SelectableAuthorViewModel(Author author)
        {
            Author = author;
        }
    }

    /// <summary>
    /// ViewModel wrapper for a Genre, used in selection UIs (e.g., ListBox with CheckBoxes).
    /// </summary>
    public partial class SelectableGenreViewModel : ObservableObject
    {
        public Genre Genre { get; }

        [ObservableProperty]
        private bool isSelected; // Binds to the CheckBox

        public SelectableGenreViewModel(Genre genre)
        {
            Genre = genre;
        }
    }
}
