using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibrary.Domain.Entities;

namespace DataAccess.Contracts
{
    public interface IAuthorRepository
    {
        Task<IEnumerable<Author>> GetAllAsync();
        public Task<Author> GetByIdAsync(int id);
        Task<int> AddAsync(Author entity);
        public Task UpdateAsync(Author author);
        public Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}