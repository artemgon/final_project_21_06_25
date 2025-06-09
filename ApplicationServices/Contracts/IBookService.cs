using Domain.Entities;
using System.Collections.Generic;

namespace BookLibrary.ApplicationServices.Contracts
{
    public interface IBookService
    {
        IEnumerable<Book> GetAllBooksWithDetails();
        Book GetBookDetails(int id);
        int CreateBook(Book book, IEnumerable<int> authorIds, IEnumerable<int> genreIds);
        void UpdateBook(Book book, IEnumerable<int> authorIds, IEnumerable<int> genreIds);
        void DeleteBook(int id);
        IEnumerable<Book> SearchBooks(string searchTerm = null, string readingStatus = null, int? genreId = null);
    }
}