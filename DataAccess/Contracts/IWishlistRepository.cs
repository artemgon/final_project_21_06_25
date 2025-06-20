// BookLibrary.DataAccess.Contracts/IWishlistRepository.cs
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Contracts
{
    public interface IWishlistRepository
    {
        Task<IEnumerable<WishlistItem>> GetAllAsync();
        Task<int> AddAsync(WishlistItem entity);
        public Task<WishlistItem> GetByIdAsync(int id);
        public Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}