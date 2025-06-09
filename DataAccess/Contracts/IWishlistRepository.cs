using Domain.Entities;
using System.Collections.Generic;

namespace BookLibrary.DataAccess.Contracts
{
    public interface IWishlistRepository
    {
        IEnumerable<WishlistItem> GetAll();
        WishlistItem GetById(int id);
        int Add(WishlistItem item);
        void Update(WishlistItem item);
        void Delete(int id);
    }
}