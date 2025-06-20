// BookLibrary.ViewModels/WishlistManagement/WishlistManagerViewModel.cs
// Make sure these using statements are present at the top of your file.
using BookLibrary.ApplicationServices.Contracts; // Assuming IWishlistService is here
using Domain.Entities; // Assuming ViewModelBase is here
using CommunityToolkit.Mvvm.ComponentModel; // Required for [ObservableProperty] and ObservableObject
using CommunityToolkit.Mvvm.Input;       // Required for [RelayCommand] and AsyncRelayCommand
using System.Collections.ObjectModel;
using System.Linq; // Required for LINQ methods like OrderBy, Where
using System.Threading.Tasks; // Required for Task, async/await
using System.Windows; // Required for MessageBox (for temporary feedback)
using System;
using ViewModels;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand; // Required for Exception and StringComparison

namespace BookLibrary.ViewModels.WishlistManagement // Ensure this namespace matches your folder structure
{
    // Mark WishlistManagerViewModel as 'partial' to allow CommunityToolkit.Mvvm source generation
    public partial class WishlistManagerViewModel : ViewModelBase // Assuming ViewModelBase inherits from ObservableObject
    {
        private readonly IWishlistService _wishlistService;

        // This ObservableCollection will hold ALL wishlist items fetched from the service.
        // It acts as the source for filtering and sorting.
        private ObservableCollection<WishlistItem> _wishlistItemsSource;
        public ObservableCollection<WishlistItem> WishlistItemsSource
        {
            get => _wishlistItemsSource;
            set
            {
                // When the source list changes (e.g., after loading from DB),
                // we update the private field and then re-apply the current filter and sort.
                SetProperty(ref _wishlistItemsSource, value);
                ApplyFilterAndSort(); // Crucial to update the UI-bound collection
            }
        }

        // This ObservableCollection will be bound directly to the UI (e.g., DataGrid).
        // It contains only the items that pass the current search filter and are currently sorted.
        public ObservableCollection<WishlistItem> FilteredWishlistItems { get; } = new ObservableCollection<WishlistItem>();

        // Property to hold the currently selected wishlist item in the UI
        [ObservableProperty]
        private WishlistItem selectedWishlistItem;

        // Property for the dynamic search term
        [ObservableProperty]
        private string searchTerm;

        // Property to control the visibility of the loading indicator
        [ObservableProperty]
        private bool isLoading;

        // Commands for sorting the wishlist items
        public RelayCommand SortByIdCommand { get; }
        public RelayCommand SortByTitleCommand { get; }
        public RelayCommand SortByAuthorCommand { get; }

        // Stores the currently active sort property, to re-apply after filtering
        private string currentSortProperty = nameof(WishlistItem.WishlistItemId); // Default sort
        private bool isAscending = true; // Track sort direction


        public WishlistManagerViewModel(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
            // Initialize the backing field for 'WishlistItemsSource' (the full list)
            _wishlistItemsSource = new ObservableCollection<WishlistItem>(); // Use _wishlistItemsSource directly here

            // Initialize sorting commands
            SortByIdCommand = new RelayCommand(() => SortWishlistItems(nameof(WishlistItem.WishlistItemId)));
            SortByTitleCommand = new RelayCommand(() => SortWishlistItems(nameof(WishlistItem.Title)));
            SortByAuthorCommand = new RelayCommand(() => SortWishlistItems(nameof(WishlistItem.Author)));


            // Subscribe to property changes:
            PropertyChanged += (sender, e) =>
            {
                // When SearchTerm changes, re-apply the filter and sort to update the displayed list
                if (e.PropertyName == nameof(SearchTerm))
                {
                    ApplyFilterAndSort();
                }
                // When SelectedWishlistItem changes, re-evaluate Delete command's CanExecute state
                if (e.PropertyName == nameof(SelectedWishlistItem))
                {
                    DeleteWishlistItemCommand.NotifyCanExecuteChanged();
                }
            };

            // Load wishlist items when the ViewModel is initialized
            _ = LoadWishlistItemsAsync();
        }

        // --- Predicates for Command Execution ---

        /// <summary>
        /// Determines if the DeleteWishlistItemCommand can be executed.
        /// Requires a wishlist item to be selected.
        /// </summary>
        private bool CanDeleteWishlistItem()
        {
            return SelectedWishlistItem != null;
        }

        // --- Asynchronous Methods (command implementations) ---

        /// <summary>
        /// Loads all wishlist items from the database and updates the source collection.
        /// This will automatically trigger ApplyFilterAndSort due to the setter of WishlistItemsSource.
        /// </summary>
        [RelayCommand]
        private async Task LoadWishlistItemsAsync()
        {
            IsLoading = true; // Show loading indicator
            try
            {
                var items = await _wishlistService.GetAllWishlistItemsAsync(); // Fetch all items
                // Update the source collection. The setter will call ApplyFilterAndSort().
                WishlistItemsSource = new ObservableCollection<WishlistItem>(items);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading wishlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false; // Hide loading indicator
                DeleteWishlistItemCommand.NotifyCanExecuteChanged(); // Re-evaluate command state
            }
        }

        /// <summary>
        /// Deletes the currently selected wishlist item from the database.
        /// </summary>
        [RelayCommand]
        private async Task DeleteWishlistItemAsync()
        {
            if (!CanDeleteWishlistItem()) return; // Pre-check validation

            if (MessageBox.Show($"Are you sure you want to remove '{SelectedWishlistItem.Title}' from your wishlist?",
                                "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                IsLoading = true; // Show loading indicator
                try
                {
                    await _wishlistService.DeleteWishlistItemAsync(SelectedWishlistItem.WishlistItemId); // Delete from DB
                    await LoadWishlistItemsAsync(); // Reload and re-filter/sort the list
                    SelectedWishlistItem = null; // Clear selection after deletion
                    MessageBox.Show("Item removed from wishlist successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error removing item from wishlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false; // Hide loading indicator
                }
            }
        }

        // --- Filtering and Sorting Logic ---

        /// <summary>
        /// Applies the current filter (SearchTerm) and then the current sort order
        /// to the WishlistItemsSource, populating FilteredWishlistItems.
        /// This method is called when SearchTerm changes or when a sort button is clicked.
        /// </summary>
        private void ApplyFilterAndSort()
        {
            FilteredWishlistItems.Clear(); // Clear the currently displayed items

            // 1. Apply Filtering
            var query = WishlistItemsSource.AsEnumerable(); // Start with all items from the source

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                string lowerSearchTerm = SearchTerm.ToLower();
                query = query.Where(item =>
                    (item.Title != null && item.Title.ToLower().Contains(lowerSearchTerm)) ||
                    (item.Author != null && item.Author.ToLower().Contains(lowerSearchTerm)) ||
                    (item.Notes != null && item.Notes.ToLower().Contains(lowerSearchTerm)) || // Search notes too
                    (item.ISBN != null && item.ISBN.ToLower().Contains(lowerSearchTerm))); // Search ISBN too
            }

            // 2. Apply Sorting to the filtered results
            IOrderedEnumerable<WishlistItem> sortedFiltered = null;

            switch (currentSortProperty)
            {
                case nameof(WishlistItem.WishlistItemId):
                    sortedFiltered = isAscending ? query.OrderBy(item => item.WishlistItemId) : query.OrderByDescending(item => item.WishlistItemId);
                    break;
                case nameof(WishlistItem.Title):
                    sortedFiltered = isAscending ? query.OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase) : query.OrderByDescending(item => item.Title, StringComparer.OrdinalIgnoreCase);
                    break;
                case nameof(WishlistItem.Author): // Assuming Author is a single string property for simplicity
                    sortedFiltered = isAscending ? query.OrderBy(item => item.Author, StringComparer.OrdinalIgnoreCase) : query.OrderByDescending(item => item.Author, StringComparer.OrdinalIgnoreCase);
                    break;
                default:
                    // Default sort if property is not recognized or not yet set (shouldn't happen with default init)
                    sortedFiltered = query.OrderBy(item => item.WishlistItemId);
                    break;
            }

            // 3. Populate the UI-bound collection with the filtered and sorted results
            foreach (var item in sortedFiltered)
            {
                FilteredWishlistItems.Add(item);
            }
        }


        /// <summary>
        /// Sets the sort property and direction, then triggers filtering and sorting.
        /// </summary>
        /// <param name="propertyName">The name of the property to sort by (e.g., "WishlistItemId", "Title", "Author").</param>
        private void SortWishlistItems(string propertyName)
        {
            if (currentSortProperty == propertyName)
            {
                isAscending = !isAscending; // Toggle direction if sorting by the same property again
            }
            else
            {
                currentSortProperty = propertyName;
                isAscending = true; // Default to ascending for a new column
            }

            ApplyFilterAndSort(); // Re-apply filter and then sort with the new criteria
        }
    }
}
