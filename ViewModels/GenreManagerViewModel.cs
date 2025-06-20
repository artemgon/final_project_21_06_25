// BookLibrary.ViewModels/GenreManagement/GenreManagerViewModel.cs
// Make sure these using statements are present at the top of your file.
using BookLibrary.ApplicationServices.Contracts; // Corrected namespace for IGenreService
using Domain.Entities; // Assuming ViewModelBase is in this namespace
using CommunityToolkit.Mvvm.ComponentModel; // Required for [ObservableProperty] and ObservableObject
using CommunityToolkit.Mvvm.Input;       // Required for [RelayCommand] and AsyncRelayCommand
using System.Collections.ObjectModel;
using System.Linq; // Required for LINQ methods like OrderBy
using System.Threading.Tasks; // Required for Task, async/await
using System.Windows; // Required for MessageBox (for temporary feedback)
using System;
using ApplicationServices.Contracts;
using ViewModels;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand; // Required for Exception and StringComparison

namespace BookLibrary.ViewModels.GenreManagement // Ensure this namespace matches your folder structure
{
    // Mark GenreManagerViewModel as 'partial' to allow CommunityToolkit.Mvvm source generation
    public partial class GenreManagerViewModel : ViewModelBase // Assuming ViewModelBase inherits from ObservableObject
    {
        private readonly IGenreService _genreService; // Dependency for genre-related operations

        // ObservableCollection to hold the list of genres displayed in the UI
        [ObservableProperty]
        private ObservableCollection<Genre> genres;

        // Property to hold the currently selected genre in the UI (e.g., DataGrid)
        [ObservableProperty]
        private Genre selectedGenre;

        // Property for the input field when adding a new genre
        [ObservableProperty]
        private string newGenreName;

        // Property to control the visibility of the loading indicator
        [ObservableProperty]
        private bool isLoading;

        // Commands for sorting the genres list
        public RelayCommand SortByGenreIdCommand { get; }
        public RelayCommand SortByGenreNameCommand { get; }


        // Constructor: Services are injected here by the Dependency Injection container
        public GenreManagerViewModel(IGenreService genreService)
        {
            _genreService = genreService;
            // Initialize the backing field for 'Genres'
            genres = new ObservableCollection<Genre>();
            
            // Initialize sorting commands
            SortByGenreIdCommand = new RelayCommand(() => SortGenres(nameof(Genre.GenreId)));
            SortByGenreNameCommand = new RelayCommand(() => SortGenres(nameof(Genre.GenreName)));

            // Subscribe to property changes to notify commands when their CanExecute state might change.
            PropertyChanged += (sender, e) =>
            {
                // When SelectedGenre changes, re-evaluate Save and Delete commands
                if (e.PropertyName == nameof(SelectedGenre))
                {
                    SaveGenreCommand.NotifyCanExecuteChanged();
                    DeleteGenreCommand.NotifyCanExecuteChanged();
                }
                // When NewGenreName changes, re-evaluate Add command
                if (e.PropertyName == nameof(NewGenreName))
                {
                    AddGenreCommand.NotifyCanExecuteChanged();
                }
            };

            // Load existing genres when the ViewModel is initialized (fire and forget)
            _ = LoadGenresAsync();
        }

        // --- Predicates for Command Execution (to enable/disable buttons) ---

        /// <summary>
        /// Determines if the AddGenreCommand can be executed.
        /// Requires the NewGenreName to be non-empty.
        /// </summary>
        private bool CanAddGenre()
        {
            return !string.IsNullOrWhiteSpace(NewGenreName);
        }

        /// <summary>
        /// Determines if the SaveGenreCommand can be executed.
        /// Requires a genre to be selected AND its name to be non-empty.
        /// </summary>
        private bool CanSaveGenre()
        {
            return SelectedGenre != null && !string.IsNullOrWhiteSpace(SelectedGenre.GenreName);
        }

        /// <summary>
        /// Determines if the DeleteGenreCommand can be executed.
        /// Requires a genre to be selected.
        /// </summary>
        private bool CanDeleteGenre()
        {
            return SelectedGenre != null;
        }

        // --- Asynchronous Methods (command implementations) ---

        /// <summary>
        /// Loads all genres from the database and populates the Genres collection.
        /// Shows/hides a loading indicator during the operation.
        /// </summary>
        [RelayCommand]
        private async Task LoadGenresAsync()
        {
            IsLoading = true; // Show loading indicator
            try
            {
                var loadedGenres = await _genreService.GetAllGenresAsync(); // Call service to get genres
                Genres.Clear(); // Clear existing items in the ObservableCollection
                foreach (var genre in loadedGenres)
                {
                    Genres.Add(genre); // Add newly loaded genres
                }
            }
            catch (Exception ex)
            {
                // Display any errors that occur during loading
                MessageBox.Show($"Error loading genres: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false; // Hide loading indicator
                // Notify commands that their CanExecute state might have changed (e.g., if the list became empty)
                DeleteGenreCommand.NotifyCanExecuteChanged();
                SaveGenreCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Adds a new genre to the database based on input field.
        /// Shows/hides a loading indicator and refreshes the list upon success.
        /// </summary>
        [RelayCommand]
        private async Task AddGenreAsync()
        {
            if (!CanAddGenre()) return; // Pre-check validation

            IsLoading = true; // Show loading indicator
            try
            {
                var newGenre = new Genre
                {
                    GenreName = NewGenreName
                };

                await _genreService.AddGenreAsync(newGenre); // Call service to add the genre to DB

                // Refresh the list to display the newly added genre
                await LoadGenresAsync();

                // Clear input field after successful addition
                NewGenreName = string.Empty;

                MessageBox.Show("Genre added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding genre: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false; // Hide loading indicator
            }
        }

        /// <summary>
        /// Saves changes to the currently selected genre in the database.
        /// Shows/hides a loading indicator and refreshes the list.
        /// </summary>
        [RelayCommand]
        private async Task SaveGenreAsync()
        {
            if (!CanSaveGenre()) return; // Pre-check validation

            IsLoading = true; // Show loading indicator
            try
            {
                await _genreService.UpdateGenreAsync(SelectedGenre); // Call service to update the genre in DB
                await LoadGenresAsync(); // Refresh the list
                MessageBox.Show("Genre updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving genre: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false; // Hide loading indicator
            }
        }

        /// <summary>
        /// Deletes the currently selected genre from the database.
        /// Shows/hides a loading indicator and refreshes the list.
        /// </summary>
        [RelayCommand]
        private async Task DeleteGenreAsync()
        {
            if (!CanDeleteGenre()) return; // Pre-check validation

            if (MessageBox.Show($"Are you sure you want to delete '{SelectedGenre.GenreName}'?",
                                "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                IsLoading = true; // Show loading indicator
                try
                {
                    await _genreService.DeleteGenreAsync(SelectedGenre.GenreId); // Call service to delete by ID
                    await LoadGenresAsync(); // Refresh the list
                    SelectedGenre = null; // Clear selection after deletion
                    MessageBox.Show("Genre deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting genre: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false; // Hide loading indicator
                }
            }
        }

        // --- Sorting Logic ---
        /// <summary>
        /// Sorts the Genres collection based on the specified property name.
        /// </summary>
        /// <param name="propertyName">The name of the property to sort by (e.g., "GenreId", "GenreName").</param>
        private void SortGenres(string propertyName)
        {
            IOrderedEnumerable<Genre> sortedGenres;

            switch (propertyName)
            {
                case nameof(Genre.GenreId):
                    sortedGenres = Genres.OrderBy(g => g.GenreId);
                    break;
                case nameof(Genre.GenreName):
                    sortedGenres = Genres.OrderBy(g => g.GenreName, StringComparer.OrdinalIgnoreCase);
                    break;
                default:
                    return; // Do nothing if an invalid property name is passed
            }

            // Clear the existing ObservableCollection and repopulate it with the sorted items.
            // This triggers UI updates efficiently.
            Genres.Clear();
            foreach (var genre in sortedGenres)
            {
                Genres.Add(genre);
            }
        }
    }
}
