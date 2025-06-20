using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookLibrary.ApplicationServices.Contracts
{
    public interface IWishlistService
    {
        Task<IEnumerable<WishlistItem>> GetAllWishlistItemsAsync();
        Task AddWishlistItemAsync(WishlistItem wishlistItem);
        Task DeleteWishlistItemAsync(int wishlistItemId);
        //Task<WishlistItem> GetWishlistItemByIdAsync(int id);
    }
}