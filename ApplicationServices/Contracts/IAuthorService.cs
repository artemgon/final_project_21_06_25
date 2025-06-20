using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibrary.Domain.Entities;

namespace ApplicationServices.Contracts
{
    public interface IAuthorService
    {
        Task<IEnumerable<Author>> GetAllAuthorsAsync(); // Renamed to async
        Task AddAuthorAsync(Author author);
        Task UpdateAuthorAsync(Author author);
        Task DeleteAuthorAsync(int authorId);
    }
}