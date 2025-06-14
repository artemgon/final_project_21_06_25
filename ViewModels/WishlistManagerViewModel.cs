using BookLibrary.ApplicationServices.Contracts;
using Domain.Entities;
using System.Collections.ObjectModel;
using System.Windows; // For MessageBox (for temporary messages)

namespace ViewModels
{
    public class WishlistManagerViewModel : ViewModelBase
    {
        private readonly IWishlistService _wishlistService;
        private ObservableCollection<WishlistItem> _wishlistItems;
        public ObservableCollection<WishlistItem> WishlistItems
        {
            get => _wishlistItems;
            set => SetProperty(ref _wishlistItems, value);
        }

        private WishlistItem _selectedWishlistItem;
        public WishlistItem SelectedWishlistItem
        {
            get => _selectedWishlistItem;
            set => SetProperty(ref _selectedWishlistItem, value);
        }

        public WishlistManagerViewModel(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
            WishlistItems = new ObservableCollection<WishlistItem>();
            LoadWishlistItems(); // Load data on initialization
        }

        private void LoadWishlistItems()
        {
            WishlistItems.Clear();
            foreach (var item in _wishlistService.GetAllWishlistItems())
            {
                WishlistItems.Add(item);
            }
        }
        // Add Add/Edit/Delete commands later
    }
}