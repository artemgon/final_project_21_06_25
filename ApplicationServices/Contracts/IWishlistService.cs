using Domain.Entities;
using System.Collections.Generic;

namespace BookLibrary.ApplicationServices.Contracts
{
    public interface IWishlistService
    {
        IEnumerable<WishlistItem> GetAllWishlistItems();
        WishlistItem GetWishlistItemById(int id);
        int AddWishlistItem(WishlistItem item);
        void UpdateWishlistItem(WishlistItem item);
        void DeleteWishlistItem(int id);
    }
}
