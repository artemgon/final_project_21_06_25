using Domain.Entities;
using System.Collections.Generic;

namespace BookLibrary.DataAccess.Contracts
{
    public interface IBookRepository
    {
        IEnumerable<Book> GetAll();
        Book GetById(int id);
        int Add(Book book);
        void Update(Book book);
        void Delete(int id);

        void AddBookAuthor(int bookId, int authorId);
        void RemoveBookAuthor(int bookId, int authorId);
        IEnumerable<Author> GetAuthorsForBook(int bookId); 
        void AddBookGenre(int bookId, int genreId);
        void RemoveBookGenre(int bookId, int genreId);
        IEnumerable<Genre> GetGenresForBook(int bookId);

        IEnumerable<Book> SearchBooks(string searchTerm = null, string readingStatus = null, int? genreId = null);
    }
}