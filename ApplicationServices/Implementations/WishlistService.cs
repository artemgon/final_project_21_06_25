using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.DataAccess.Contracts;
using Domain.Entities;
using System.Collections.Generic;

namespace BookLibrary.ApplicationServices.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;

        public WishlistService(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public IEnumerable<WishlistItem> GetAllWishlistItems()
        {
            return _wishlistRepository.GetAll();
        }

        public WishlistItem GetWishlistItemById(int id)
        {
            return _wishlistRepository.GetById(id);
        }

        public int AddWishlistItem(WishlistItem item)
        {
            return _wishlistRepository.Add(item);
        }

        public void UpdateWishlistItem(WishlistItem item)
        {
            _wishlistRepository.Update(item);
        }

        public void DeleteWishlistItem(int id)
        {
            _wishlistRepository.Delete(id);
        }
    }
}