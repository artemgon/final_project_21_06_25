// BookLibrary.ViewModels/BookManagement/BookDetailViewModel.cs
// Make sure these using statements are present at the top of your file.
using BookLibrary.ApplicationServices.Contracts; // For IBookService, IAuthorService, IGenreService
using Domain.Entities; // For Book, Author, Genre entities
using Domain.Enums; // For ReadingStatus enum
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
using BookLibrary.ViewModels.Messages;
using CommunityToolkit.Mvvm.Messaging;
using ViewModels;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand; // For DateTime, StringComparison

namespace ViewModels // Fixed namespace to match file location
{
    // Mark BookDetailViewModel as 'partial' for CommunityToolkit.Mvvm source generation
    public partial class BookDetailViewModel : ViewModelBase
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IGenreService _genreService;
        private readonly IImageService _imageService; // Add image service

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
        private string authorSearchTerm = string.Empty;
        [ObservableProperty]
        private string genreSearchTerm = string.Empty;

        // Property for URL input
        [ObservableProperty]
        private string imageUrlInput = string.Empty;

        // Filtered views of available authors/genres. These are bound directly to the UI ListBoxes.
        public ObservableCollection<SelectableAuthorViewModel> FilteredAvailableAuthors { get; } = new ObservableCollection<SelectableAuthorViewModel>();
        public ObservableCollection<SelectableGenreViewModel> FilteredAvailableGenres { get; } = new ObservableCollection<SelectableGenreViewModel>();

        // Selected items in the available lists for adding
        [ObservableProperty]
        private SelectableAuthorViewModel? selectedAvailableAuthor;
        [ObservableProperty]
        private SelectableGenreViewModel? selectedAvailableGenre;
        
        // Selected items in the current lists for removing
        [ObservableProperty]
        private Author? selectedCurrentAuthor;
        [ObservableProperty]
        private Genre? selectedCurrentGenre;

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
        // Commands to add selected authors and genres to the current book
        public RelayCommand AddAuthorCommand { get; }
        public RelayCommand AddGenreCommand { get; }
        // Commands to remove selected authors and genres from the current book
        public RelayCommand RemoveAuthorCommand { get; }
        public RelayCommand RemoveGenreCommand { get; }
        // New commands for URL functionality and image removal
        public RelayCommand LoadImageFromUrlCommand { get; }
        public RelayCommand RemoveImageCommand { get; }


        // Constructor: Services are injected here by the Dependency Injection container.
        public BookDetailViewModel(IBookService bookService, IAuthorService authorService, IGenreService genreService, IImageService imageService)
        {
            _bookService = bookService;
            _authorService = authorService;
            _genreService = genreService;
            _imageService = imageService;

            // Initialize CurrentBook with a default empty instance.
            // This ensures CurrentBook is never null and can be bound to immediately.
            currentBook = new Book
            {
                Title = "New Book", // Start with a default title so Save button works
                ISBN = string.Empty,
                PublicationYear = DateTime.Now.Year,
                PageCount = 100, // Default page count so Save button works
                ReadingStatus = ReadingStatus.NotStarted,
                Rating = null, // Keep rating optional
                Summary = string.Empty,
                CoverImagePath = string.Empty,
                Authors = new ObservableCollection<Author>(),
                Genres = new ObservableCollection<Genre>()
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
            AddAuthorCommand = new RelayCommand(AddAuthor);
            AddGenreCommand = new RelayCommand(AddGenre);
            RemoveAuthorCommand = new RelayCommand(RemoveAuthor);
            RemoveGenreCommand = new RelayCommand(RemoveGenre);
            LoadImageFromUrlCommand = new RelayCommand(LoadImageFromUrl);
            RemoveImageCommand = new RelayCommand(RemoveImage);

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
                PageCount = null, // Make it nullable/optional
                ReadingStatus = ReadingStatus.NotStarted, // Use simplified enum reference
                Rating = null, // Keep rating null instead of 0 to avoid constraint violation
                Summary = string.Empty,
                CoverImagePath = string.Empty,
                Authors = new ObservableCollection<Author>(), // Initialize empty collections for a new book
                Genres = new ObservableCollection<Genre>()
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
        /// Opens a file dialog to select a cover image for the book.
        /// Shows preview immediately for both new and existing books.
        /// </summary>
        private void ChangeCoverImage()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.webp|" +
                            "JPEG Files|*.jpg;*.jpeg|" +
                            "PNG Files|*.png|" +
                            "All Files|*.*",
                    Title = "Select Cover Image",
                    CheckFileExists = true,
                    CheckPathExists = true
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var selectedImagePath = openFileDialog.FileName;
                    
                    // Validate the image file
                    if (!_imageService.ValidateImageFile(selectedImagePath))
                    {
                        MessageBox.Show("Please select a valid image file (jpg, jpeg, png, gif, bmp).", 
                            "Invalid Image", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Show preview immediately for both new and existing books
                    CurrentBook.CoverImagePath = selectedImagePath;
                    OnPropertyChanged(nameof(CurrentBook));
                    
                    // For existing books, save the image in the background
                    if (!IsNewBook)
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                IsLoading = true;
                                
                                // Delete old cover image if it exists and it's not the current one
                                var oldPath = CurrentBook.CoverImagePath;
                                if (!string.IsNullOrEmpty(oldPath) && oldPath != selectedImagePath)
                                {
                                    await _imageService.DeleteBookCoverAsync(oldPath);
                                }

                                // Save new cover image
                                var relativePath = await _imageService.SaveBookCoverAsync(selectedImagePath, CurrentBook.BookId);
                                
                                // Update the book's cover image path
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CurrentBook.CoverImagePath = relativePath;
                                    OnPropertyChanged(nameof(CurrentBook));
                                });

                                // Update the book in the database
                                await _bookService.UpdateBookAsync(CurrentBook);
                                
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show("Cover image updated successfully!", "Success", 
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                });
                            }
                            catch (Exception ex)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show($"Error updating cover image: {ex.Message}", "Error", 
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                });
                            }
                            finally
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    IsLoading = false;
                                });
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsLoading = false;
            }
        }

        /// <summary>
        /// Loads an image from the specified URL and sets it as the book's cover image.
        /// Shows preview immediately for both new and existing books.
        /// </summary>
        private void LoadImageFromUrl()
        {
            if (string.IsNullOrWhiteSpace(ImageUrlInput))
            {
                MessageBox.Show("Please enter a valid image URL.", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Show preview immediately for both new and existing books
            CurrentBook.CoverImagePath = ImageUrlInput;
            OnPropertyChanged(nameof(CurrentBook));
            
            // For existing books, download and save the image in the background
            if (!IsNewBook)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        IsLoading = true;
                        
                        // Delete old cover image if it exists
                        var oldPath = CurrentBook.CoverImagePath;
                        if (!string.IsNullOrEmpty(oldPath) && oldPath != ImageUrlInput)
                        {
                            await _imageService.DeleteBookCoverAsync(oldPath);
                        }

                        // Download and save the new image
                        var downloadedImagePath = await _imageService.DownloadImageAsync(ImageUrlInput);
                        var relativePath = await _imageService.SaveBookCoverAsync(downloadedImagePath, CurrentBook.BookId);
                        
                        // Update the book's cover image path
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CurrentBook.CoverImagePath = relativePath;
                            OnPropertyChanged(nameof(CurrentBook));
                        });

                        // Update the book in the database
                        await _bookService.UpdateBookAsync(CurrentBook);
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Cover image updated successfully!", "Success", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error updating cover image: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsLoading = false;
                        });
                    }
                });
            }
        }

        /// <summary>
        /// Removes the cover image from the book. For new books, clears the URL. For existing books, deletes the image file.
        /// </summary>
        private void RemoveImage()
        {
            if (IsNewBook)
            {
                // For new books, just clear the URL
                CurrentBook.CoverImagePath = string.Empty;
                MessageBox.Show("Cover image removed. The book will be saved without a cover image.", 
                                "Image Removed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // For existing books, delete the image file
                Task.Run(async () =>
                {
                    try
                    {
                        // Delete the cover image file
                        await _imageService.DeleteBookCoverAsync(CurrentBook.CoverImagePath);

                        // Clear the cover image path in the book
                        CurrentBook.CoverImagePath = string.Empty;

                        // Update the book in the database
                        await _bookService.UpdateBookAsync(CurrentBook);
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Cover image removed successfully!", "Success", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error removing cover image: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsLoading = false;
                        });
                    }
                });
            }

            OnPropertyChanged(nameof(CurrentBook));
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

            // Title cannot be empty (this is the only required field)
            if (string.IsNullOrWhiteSpace(CurrentBook.Title))
                return false;

            // All other validations are optional - let's be more permissive
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
        /// </summary>
        private void Cancel()
        {
            // Send a message to navigate back to the book list
            WeakReferenceMessenger.Default.Send(new NavigateToBookListMessage());
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

            AllAvailableGenres.Clear();
            FilteredAvailableGenres.Clear(); // Clear filtered list too
            var genres = await _genreService.GetAllGenresAsync();
            foreach (var genre in genres.OrderBy(g => g.GenreName)) // Order for display
            {
                var selectableGenre = new SelectableGenreViewModel(genre);
                AllAvailableGenres.Add(selectableGenre);
                FilteredAvailableGenres.Add(selectableGenre); // Initially, all are in the filtered list
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
            if (CurrentBook.Authors == null) CurrentBook.Authors = new ObservableCollection<Author>();
            if (CurrentBook.Genres == null) CurrentBook.Genres = new ObservableCollection<Genre>();

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

        /// <summary>
        /// Adds the selected author from the available authors list to the current book.
        /// </summary>
        private void AddAuthor()
        {
            if (SelectedAvailableAuthor != null && !CurrentBook.Authors.Any(a => a.AuthorId == SelectedAvailableAuthor.Author.AuthorId))
            {
                CurrentBook.Authors.Add(SelectedAvailableAuthor.Author);
                SelectedAvailableAuthor.IsSelected = true; // Keep the author selected in the UI
            }
        }

        /// <summary>
        /// Adds the selected genre from the available genres list to the current book.
        /// </summary>
        private void AddGenre()
        {
            if (SelectedAvailableGenre != null && !CurrentBook.Genres.Any(g => g.GenreId == SelectedAvailableGenre.Genre.GenreId))
            {
                CurrentBook.Genres.Add(SelectedAvailableGenre.Genre);
                SelectedAvailableGenre.IsSelected = true; // Keep the genre selected in the UI
            }
        }

        /// <summary>
        /// Removes the selected author from the current book.
        /// </summary>
        private void RemoveAuthor()
        {
            if (SelectedCurrentAuthor == null)
            {
                System.Diagnostics.Debug.WriteLine("RemoveAuthor: SelectedCurrentAuthor is null, aborting");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"RemoveAuthor: Removing {SelectedCurrentAuthor.FirstName} {SelectedCurrentAuthor.LastName}");
            
            // Store the author ID before removing to avoid null reference later
            var authorIdToRemove = SelectedCurrentAuthor.AuthorId;
            
            // Remove from current book's authors collection
            CurrentBook.Authors.Remove(SelectedCurrentAuthor);
            
            // Find the corresponding SelectableAuthorViewModel and unselect it
            // Add comprehensive null checks
            if (AllAvailableAuthors != null && AllAvailableAuthors.Any())
            {
                var selectableAuthor = AllAvailableAuthors.FirstOrDefault(a => a != null && a.Author != null && a.Author.AuthorId == authorIdToRemove);
                if (selectableAuthor != null)
                {
                    selectableAuthor.IsSelected = false;
                    System.Diagnostics.Debug.WriteLine($"RemoveAuthor: Unmarked {selectableAuthor.FullName} as selected");
                }
            }
            
            // Clear the selection
            SelectedCurrentAuthor = null;
            
            System.Diagnostics.Debug.WriteLine($"RemoveAuthor: Current authors count: {CurrentBook.Authors.Count}");
        }

        /// <summary>
        /// Removes the selected genre from the current book.
        /// </summary>
        private void RemoveGenre()
        {
            if (SelectedCurrentGenre == null)
            {
                System.Diagnostics.Debug.WriteLine("RemoveGenre: SelectedCurrentGenre is null, aborting");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"RemoveGenre: Removing {SelectedCurrentGenre.GenreName}");
            
            // Store the genre ID before removing to avoid null reference later
            var genreIdToRemove = SelectedCurrentGenre.GenreId;
            
            // Remove from current book's genres collection
            CurrentBook.Genres.Remove(SelectedCurrentGenre);
            
            // Find the corresponding SelectableGenreViewModel and unselect it
            // Add comprehensive null checks
            if (AllAvailableGenres != null && AllAvailableGenres.Any())
            {
                var selectableGenre = AllAvailableGenres.FirstOrDefault(g => g != null && g.Genre != null && g.Genre.GenreId == genreIdToRemove);
                if (selectableGenre != null)
                {
                    selectableGenre.IsSelected = false;
                    System.Diagnostics.Debug.WriteLine($"RemoveGenre: Unmarked {selectableGenre.Genre.GenreName} as selected");
                }
            }
            
            // Clear the selection
            SelectedCurrentGenre = null;
            
            System.Diagnostics.Debug.WriteLine($"RemoveGenre: Current genres count: {CurrentBook.Genres.Count}");
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
