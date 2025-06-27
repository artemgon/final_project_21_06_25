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
            try
            {
                System.Diagnostics.Debug.WriteLine("AuthorService.GetAllAuthorsAsync: Starting...");
                var authors = await _authorRepository.GetAllAsync();
                var authorsList = authors?.ToList() ?? new List<Author>();
                System.Diagnostics.Debug.WriteLine($"AuthorService.GetAllAuthorsAsync: Retrieved {authorsList.Count} authors from repository");
                
                foreach (var author in authorsList)
                {
                    System.Diagnostics.Debug.WriteLine($"  Author: ID={author.AuthorId}, Name={author.FirstName} {author.LastName}");
                }
                
                return authorsList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuthorService.GetAllAuthorsAsync ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task AddAuthorAsync(Author author)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AuthorService.AddAuthorAsync: Adding author {author.FirstName} {author.LastName}");
                var newId = await _authorRepository.AddAsync(author);
                System.Diagnostics.Debug.WriteLine($"AuthorService.AddAuthorAsync: New author ID = {newId}");
                await _authorRepository.SaveChangesAsync(); // Commit changes to DB
                System.Diagnostics.Debug.WriteLine("AuthorService.AddAuthorAsync: Changes saved to database");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuthorService.AddAuthorAsync ERROR: {ex.Message}");
                throw;
            }
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