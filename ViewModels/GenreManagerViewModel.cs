using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.ComponentModel;
using ApplicationServices.Contracts;
using Domain.Entities;
using ViewModels;

namespace BookLibrary.ViewModels.GenreManagement
{
    public partial class GenreManagerViewModel : ViewModelBase
    {
        private readonly IGenreService _genreService;

        [ObservableProperty]
        private ObservableCollection<Genre> genres;

        [ObservableProperty]
        private Genre selectedGenre;

        [ObservableProperty]
        private string newGenreName;

        [ObservableProperty]
        private string newGenreDescription;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool canSaveGenre;

        // Manual command implementations to ensure they work properly
        private AsyncRelayCommand _addGenreCommand;
        public AsyncRelayCommand AddGenreCommand
        {
            get
            {
                if (_addGenreCommand == null)
                {
                    _addGenreCommand = new AsyncRelayCommand(AddGenreAsync, CanAddGenre);
                    System.Diagnostics.Debug.WriteLine("AddGenreCommand created manually");
                }
                return _addGenreCommand;
            }
        }

        private AsyncRelayCommand _deleteGenreCommand;
        public AsyncRelayCommand DeleteGenreCommand
        {
            get
            {
                if (_deleteGenreCommand == null)
                {
                    _deleteGenreCommand = new AsyncRelayCommand(DeleteGenreAsync, CanDeleteGenre);
                    System.Diagnostics.Debug.WriteLine("DeleteGenreCommand created manually");
                }
                return _deleteGenreCommand;
            }
        }

        // Add partial methods to handle property changes for new genre fields
        partial void OnNewGenreNameChanged(string value)
        {
            System.Diagnostics.Debug.WriteLine($"NewGenreName changed to: '{value}'");
            AddGenreCommand?.NotifyCanExecuteChanged();
        }

        partial void OnNewGenreDescriptionChanged(string value)
        {
            System.Diagnostics.Debug.WriteLine($"NewGenreDescription changed to: '{value}'");
            AddGenreCommand?.NotifyCanExecuteChanged();
        }

        // Add partial method to handle SelectedGenre changes and update DeleteGenreCommand
        partial void OnSelectedGenreChanged(Genre value)
        {
            System.Diagnostics.Debug.WriteLine($"SelectedGenre changed to: {value?.GenreName}");
            DeleteGenreCommand?.NotifyCanExecuteChanged();
            System.Diagnostics.Debug.WriteLine($"DeleteGenreCommand.CanExecute after change: {DeleteGenreCommand?.CanExecute(null)}");
        }

        private string currentSortProperty = nameof(Genre.GenreId);
        private bool isAscending = true;

        public GenreManagerViewModel(IGenreService genreService)
        {
            _genreService = genreService;
            genres = new ObservableCollection<Genre>();
            canSaveGenre = false;

            // Subscribe to property changes
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SelectedGenre))
                {
                    SaveGenreCommand.NotifyCanExecuteChanged();
                    DeleteGenreCommand.NotifyCanExecuteChanged();
                }
                else if (e.PropertyName == nameof(NewGenreName) ||
                         e.PropertyName == nameof(NewGenreDescription))
                {
                    AddGenreCommand.NotifyCanExecuteChanged();
                }
            };
            
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GenreManagerViewModel: Loading genres...");
                await LoadGenresAsync();
                System.Diagnostics.Debug.WriteLine($"GenreManagerViewModel: Loaded {Genres.Count} genres");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GenreManagerViewModel error: {ex.Message}");
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        partial void OnSelectedGenreChanged(Genre oldValue, Genre newValue)
        {
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= SelectedGenre_PropertyChanged;
            }

            if (newValue != null)
            {
                newValue.PropertyChanged += SelectedGenre_PropertyChanged;
                UpdateCanSaveGenre();
            }
            else
            {
                CanSaveGenre = false;
            }
        }
        
        private void SelectedGenre_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Genre.GenreName) ||
                e.PropertyName == nameof(Genre.Description))
            {
                UpdateCanSaveGenre();
                SaveGenreCommand.NotifyCanExecuteChanged();
            }
        }

        // --- Predicates for Command Execution ---
        private bool CanAddGenre()
        {
            return !string.IsNullOrWhiteSpace(NewGenreName);
        }

        private bool CanSaveGenreFunc()
        {
            return SelectedGenre != null && 
                   !string.IsNullOrWhiteSpace(SelectedGenre.GenreName);
        }

        private bool CanDeleteGenre()
        {
            return SelectedGenre != null;
        }

        // --- Commands ---
        [RelayCommand]
        private async Task LoadGenresAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadGenresAsync: Starting to load genres from database...");
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                });
                
                var loadedGenres = await _genreService.GetAllGenresAsync();
                
                System.Diagnostics.Debug.WriteLine($"LoadGenresAsync: Retrieved {loadedGenres?.Count() ?? 0} genres from service");
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Genres.Clear();
                    
                    if (loadedGenres != null)
                    {
                        foreach (var genre in loadedGenres)
                        {
                            Genres.Add(genre);
                            System.Diagnostics.Debug.WriteLine($"Added genre to UI: {genre.GenreName} (ID: {genre.GenreId})");
                        }
                    }
                    
                    OnPropertyChanged(nameof(Genres));
                });
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (Genres.Count > 0)
                    {
                        SortGenres(currentSortProperty, false);
                    }
                });
                
                System.Diagnostics.Debug.WriteLine($"LoadGenresAsync: Completed successfully. Total genres in UI: {Genres.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadGenresAsync error: {ex.Message}");
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Error loading genres: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                    SaveGenreCommand?.NotifyCanExecuteChanged();
                    DeleteGenreCommand?.NotifyCanExecuteChanged();
                });
            }
        }

        // Remove the [RelayCommand] attribute since we're manually implementing AddGenreCommand
        private async Task AddGenreAsync()
        {
            if (!CanAddGenre()) return;

            IsLoading = true;
            try
            {
                var newGenre = new Genre
                {
                    GenreName = NewGenreName,
                    Description = NewGenreDescription
                };

                System.Diagnostics.Debug.WriteLine($"Adding genre: {newGenre.GenreName}");
                await _genreService.AddGenreAsync(newGenre);
                System.Diagnostics.Debug.WriteLine("Genre added to database successfully");
                
                NewGenreName = string.Empty;
                NewGenreDescription = string.Empty;
                
                await LoadGenresAsync();
                
                OnPropertyChanged(nameof(Genres));
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Genres.Any())
                    {
                        var addedGenre = Genres.OrderByDescending(g => g.GenreId).FirstOrDefault();
                        if (addedGenre != null)
                        {
                            SelectedGenre = addedGenre;
                        }
                    }
                });
                
                MessageBox.Show($"Genre added successfully! Total genres: {Genres.Count}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding genre: {ex.Message}");
                MessageBox.Show($"Error adding genre: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveGenreAsync()
        {
            if (!CanSaveGenreFunc()) return;

            IsLoading = true;
            try
            {
                await _genreService.UpdateGenreAsync(SelectedGenre);
                await LoadGenresAsync();
                MessageBox.Show("Genre updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving genre: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Remove the [RelayCommand] attribute since we're manually implementing DeleteGenreCommand
        private async Task DeleteGenreAsync()
        {
            if (!CanDeleteGenre()) return;

            if (MessageBox.Show($"Are you sure you want to delete '{SelectedGenre.GenreName}'?",
                                "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _genreService.DeleteGenreAsync(SelectedGenre.GenreId);
                    await LoadGenresAsync();
                    SelectedGenre = null;
                    MessageBox.Show("Genre deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting genre: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        [RelayCommand]
        private void SortById() => SortGenres(nameof(Genre.GenreId));

        [RelayCommand]
        private void SortByName() => SortGenres(nameof(Genre.GenreName));

        private void SortGenres(string propertyName, bool toggleDirection = true)
        {
            if (Genres == null || Genres.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("SortGenres: No genres to sort, skipping.");
                return;
            }

            if (currentSortProperty == propertyName && toggleDirection)
            {
                isAscending = !isAscending;
            }
            else
            {
                currentSortProperty = propertyName;
                isAscending = true;
            }

            System.Diagnostics.Debug.WriteLine($"SortGenres: Sorting {Genres.Count} genres by {propertyName}, ascending: {isAscending}");

            IOrderedEnumerable<Genre> sortedGenres;

            switch (currentSortProperty)
            {
                case nameof(Genre.GenreId):
                    sortedGenres = isAscending ? Genres.OrderBy(g => g.GenreId) : Genres.OrderByDescending(g => g.GenreId);
                    break;
                case nameof(Genre.GenreName):
                    sortedGenres = isAscending ? Genres.OrderBy(g => g.GenreName, StringComparer.OrdinalIgnoreCase)
                                                : Genres.OrderByDescending(g => g.GenreName, StringComparer.OrdinalIgnoreCase);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"SortGenres: Unknown property {propertyName}, keeping original order");
                    return;
            }

            var sortedList = sortedGenres.ToList();
            System.Diagnostics.Debug.WriteLine($"SortGenres: Created sorted list with {sortedList.Count} genres");

            Genres.Clear();
            foreach (var genre in sortedList)
            {
                Genres.Add(genre);
            }
            
            System.Diagnostics.Debug.WriteLine($"SortGenres: Completed. Final Genres count: {Genres.Count}");
        }

        private void UpdateCanSaveGenre()
        {
            if (SelectedGenre == null)
            {
                CanSaveGenre = false;
                return;
            }

            bool genreNameFilled = !string.IsNullOrWhiteSpace(SelectedGenre.GenreName);
            
            System.Diagnostics.Debug.WriteLine($"UpdateCanSaveGenre: GenreName filled: {genreNameFilled}");
            
            if (CanSaveGenre == genreNameFilled)
            {
                CanSaveGenre = !genreNameFilled;
            }
            CanSaveGenre = genreNameFilled;
            
            SaveGenreCommand?.NotifyCanExecuteChanged();
        }

        partial void OnCanSaveGenreChanged(bool value)
        {
            System.Diagnostics.Debug.WriteLine($"CanSaveGenre property changed to: {value}");
            OnPropertyChanged(nameof(CanSaveGenre));
        }
    }
}
