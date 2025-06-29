// BookLibrary.ApplicationServices.Implementations/BookService.cs
using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.DataAccess.Contracts;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace BookLibrary.ApplicationServices.Implementations
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<Book>> GetAllBooksWithDetailsAsync()
        {
            // This method should load books including their Authors and Genres
            return await _bookRepository.GetAllWithDetailsAsync(); // Assuming this method exists in your repo
        }

        public async Task<Book> GetBookDetailsAsync(int bookId)
        {
            // This should load a single book including its Authors and Genres
            return await _bookRepository.GetByIdWithDetailsAsync(bookId);
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm, string readingStatus, int? genreId)
        {
            return await _bookRepository.SearchAsync(searchTerm, readingStatus, genreId);
        }

        public async Task AddBookAsync(Book book)
        {
            // Check if a book with the same ISBN already exists
            if (!string.IsNullOrEmpty(book.ISBN))
            {
                var allBooks = await _bookRepository.GetAllWithDetailsAsync();
                var existingBook = allBooks.FirstOrDefault(b => b.ISBN == book.ISBN);
                if (existingBook != null)
                {
                    throw new InvalidOperationException($"A book with ISBN '{book.ISBN}' already exists: '{existingBook.Title}'");
                }
            }

            await _bookRepository.AddAsync(book);
            await _bookRepository.SaveChangesAsync();
        }

        public async Task UpdateBookAsync(Book book)
        {
            await _bookRepository.UpdateAsync(book);
            await _bookRepository.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int bookId)
        {
            await _bookRepository.DeleteAsync(bookId);
            await _bookRepository.SaveChangesAsync();
        }
    }
}