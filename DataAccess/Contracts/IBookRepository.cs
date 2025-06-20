using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibrary.Domain.Entities;

namespace BookLibrary.DataAccess.Contracts
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync(); // Existing
        public Task<IEnumerable<Book>> GetAllWithDetailsAsync();
        public Task<Book> GetByIdAsync(int id);
        Task<IEnumerable<Book>> SearchAsync(string searchTerm, string readingStatus, int? genreId); // Updated
        public Task<Book> GetByIdWithDetailsAsync(int id); // New method to get book with details
        Task<int> AddAsync(Book entity);
        public Task UpdateAsync(Book book);
        public Task DeleteAsync(int id);
        public Task AddBookAuthorAsync(int bookId, int authorId);
        public Task<IEnumerable<Author>> GetAuthorsForBookAsync(int bookId);
        public Task RemoveBookAuthorAsync(int bookId, int authorId);
        public Task AddBookGenreAsync(int bookId, int genreId);
        public Task RemoveBookGenreAsync(int bookId, int genreId);
        public Task<IEnumerable<Genre>> GetGenresForBookAsync(int bookId);
        Task SaveChangesAsync();
    }
}