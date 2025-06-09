using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.DataAccess.Contracts;
using BookLibrary.DataAccess.Implementations;
using DataAccess.Contracts;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq; 

namespace BookLibrary.ApplicationServices.Implementations
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IGenreRepository _genreRepository; 

        public BookService(IBookRepository bookRepository, IAuthorRepository authorRepository, IGenreRepository genreRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _genreRepository = genreRepository;
        }

        public IEnumerable<Book> GetAllBooksWithDetails()
        {
            var books = _bookRepository.GetAll().ToList();

            foreach (var book in books)
            {
                book.Authors = _bookRepository.GetAuthorsForBook(book.BookId).ToList();
                book.Genres = _bookRepository.GetGenresForBook(book.BookId).ToList();
            }
            return books;
        }

        public Book GetBookDetails(int id)
        {
            var book = _bookRepository.GetById(id);
            if (book != null)
            {
                book.Authors = _bookRepository.GetAuthorsForBook(book.BookId).ToList();
                book.Genres = _bookRepository.GetGenresForBook(book.BookId).ToList();
            }
            return book;
        }

        public int CreateBook(Book book, IEnumerable<int> authorIds, IEnumerable<int> genreIds)
        {
            int newBookId = _bookRepository.Add(book);

            foreach (int authorId in authorIds)
            {
                _bookRepository.AddBookAuthor(newBookId, authorId);
            }

            foreach (int genreId in genreIds)
            {
                _bookRepository.AddBookGenre(newBookId, genreId);
            }

            return newBookId;
        }

        public void UpdateBook(Book book, IEnumerable<int> authorIds, IEnumerable<int> genreIds)
        {
            _bookRepository.Update(book);

            foreach (var author in _bookRepository.GetAuthorsForBook(book.BookId))
            {
                _bookRepository.RemoveBookAuthor(book.BookId, author.AuthorId);
            }

            foreach (int authorId in authorIds)
            {
                _bookRepository.AddBookAuthor(book.BookId, authorId);
            }

            foreach (var genre in _bookRepository.GetGenresForBook(book.BookId))
            {
                _bookRepository.RemoveBookGenre(book.BookId, genre.GenreId);
            }

            foreach (int genreId in genreIds)
            {
                _bookRepository.AddBookGenre(book.BookId, genreId);
            }
        }

        public void DeleteBook(int id)
        {
            _bookRepository.Delete(id);
        }

        public IEnumerable<Book> SearchBooks(string searchTerm = null, string readingStatus = null, int? genreId = null)
        {
            var books = _bookRepository.SearchBooks(searchTerm, readingStatus, genreId).ToList();

            foreach (var book in books)
            {
                book.Authors = _bookRepository.GetAuthorsForBook(book.BookId).ToList();
                book.Genres = _bookRepository.GetGenresForBook(book.BookId).ToList();
            }

            return books;
        }
    }
}