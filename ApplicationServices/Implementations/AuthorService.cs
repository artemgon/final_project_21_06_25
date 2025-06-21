// BookLibrary.ApplicationServices.Implementations/AuthorService.cs
using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.DataAccess.Contracts; // Assuming your repository interface is here
using BookLibrary.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationServices.Contracts;
using DataAccess.Contracts;

namespace BookLibrary.ApplicationServices.Implementations
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;

        public AuthorService(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
        {
            return await _authorRepository.GetAllAsync(); // Ensure your repository method is also async
        }

        public async Task AddAuthorAsync(Author author)
        {
            await _authorRepository.AddAsync(author);
            await _authorRepository.SaveChangesAsync(); // Commit changes to DB
        }

        public async Task UpdateAuthorAsync(Author author)
        {
            await _authorRepository.UpdateAsync(author); // Assuming this marks the entity as modified
            await _authorRepository.SaveChangesAsync();
        }

        public async Task DeleteAuthorAsync(int authorId)
        {
            // Implement retrieval and deletion, or direct delete if supported by repository
            await _authorRepository.DeleteAsync(authorId);
            await _authorRepository.SaveChangesAsync();
        }
    }
}