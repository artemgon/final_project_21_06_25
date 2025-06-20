// BookLibrary.ApplicationServices.Implementations/WishlistService.cs
using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.DataAccess.Contracts; // Assuming your repository interface is here
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Contracts;

namespace BookLibrary.ApplicationServices.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;

        public WishlistService(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public async Task<IEnumerable<WishlistItem>> GetAllWishlistItemsAsync()
        {
            return await _wishlistRepository.GetAllAsync(); // Ensure repository method is async
        }

        public async Task AddWishlistItemAsync(WishlistItem wishlistItem)
        {
            // Ensure DateAdded is set before saving
            if (wishlistItem.DateAdded == default)
            {
                wishlistItem.DateAdded = DateTime.UtcNow; // Or DateTime.Now, choose consistently
            }
            await _wishlistRepository.AddAsync(wishlistItem);
            await _wishlistRepository.SaveChangesAsync();
        }

        public async Task DeleteWishlistItemAsync(int wishlistItemId)
        {
            await _wishlistRepository.DeleteAsync(wishlistItemId);
            await _wishlistRepository.SaveChangesAsync();
        }
    }
}