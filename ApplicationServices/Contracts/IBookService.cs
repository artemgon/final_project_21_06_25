using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookLibrary.ApplicationServices.Contracts
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksWithDetailsAsync(); 
        Task<Book> GetBookDetailsAsync(int bookId); 
        Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm, string readingStatus, int? genreId); 
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(int bookId);
    }
}