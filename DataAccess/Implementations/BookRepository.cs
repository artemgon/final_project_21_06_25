using DataAccess.Contracts; 
using DataAccess.Database;
using Domain.Entities; 
using Dapper; 
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BookLibrary.DataAccess.Contracts;
using BookLibrary.Domain.Entities;

namespace DataAccess.Implementations
{
    public class BookRepository : IBookRepository
    {
        private readonly DbConnectionFactory _connectionFactory;
        private IDbConnection _connection; 

        public BookRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "SELECT * FROM Books ORDER BY Title";
                return await connection.QueryAsync<Book>(sql);
            }
        }
        
        public async Task<IEnumerable<Book>> GetAllWithDetailsAsync()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    SELECT
                        B.*,
                        A.AuthorId, A.FirstName, A.LastName, A.Biography, -- Author details
                        G.GenreId, G.GenreName -- Genre details
                    FROM Books B
                    LEFT JOIN BookAuthors BA ON B.BookId = BA.BookId
                    LEFT JOIN Authors A ON BA.AuthorId = A.AuthorId
                    LEFT JOIN BookGenres BG ON B.BookId = BG.BookId
                    LEFT JOIN Genres G ON BG.GenreId = G.GenreId
                    ORDER BY B.BookId, A.AuthorId, G.GenreId; -- Order for correct multi-mapping grouping";

                var bookDictionary = new Dictionary<int, Book>();

                // Dapper's QueryAsync with multi-mapping
                var result = await connection.QueryAsync<Book, Author, Genre, Book>(
                    sql,
                    (book, author, genre) =>
                    {
                        // Try to get the book from the dictionary, or add it if new
                        if (!bookDictionary.TryGetValue(book.BookId, out var currentBook))
                        {
                            currentBook = book;
                            currentBook.Authors = new List<Author>(); // Initialize collections
                            currentBook.Genres = new List<Genre>();
                            bookDictionary.Add(currentBook.BookId, currentBook);
                        }

                        // Add author if not null and not already added to this book's authors
                        if (author != null && !currentBook.Authors.Any(a => a.AuthorId == author.AuthorId))
                        {
                            currentBook.Authors.Add(author);
                        }

                        // Add genre if not null and not already added to this book's genres
                        if (genre != null && !currentBook.Genres.Any(g => g.GenreId == genre.GenreId))
                        {
                            currentBook.Genres.Add(genre);
                        }

                        return currentBook; // Return the book (which is being built in the dictionary)
                    },
                    splitOn: "AuthorId,GenreId" // Tells Dapper where to split the returned columns into new objects
                );

                return bookDictionary.Values; // Return the unique Book objects from the dictionary
            }
        }

        // Make sure you have 'using System.Threading.Tasks;' at the top of your file.

        public async Task<Book> GetByIdAsync(int id) // Changed return type to Task<Book> and added 'async'
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "SELECT * FROM Books WHERE BookId = @BookId";
                // Changed to QuerySingleOrDefaultAsync and added 'await'
                return await connection.QuerySingleOrDefaultAsync<Book>(sql, new { BookId = id }); 
            }
        }
        
        public async Task<Book> GetByIdWithDetailsAsync(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    SELECT
                        B.*,
                        A.AuthorId, A.FirstName, A.LastName, A.Biography,
                        G.GenreId, G.GenreName
                    FROM Books B
                    LEFT JOIN BookAuthors BA ON B.BookId = BA.BookId
                    LEFT JOIN Authors A ON BA.AuthorId = A.AuthorId
                    LEFT JOIN BookGenres BG ON B.BookId = BG.BookId
                    LEFT JOIN Genres G ON BG.GenreId = G.GenreId
                    WHERE B.BookId = @BookId
                    ORDER BY A.AuthorId, G.GenreId; -- Order for correct multi-mapping grouping";

                Book resultBook = null;

                // QueryAsync with multi-mapping for a single result (or multiple rows that map to one book)
                await connection.QueryAsync<Book, Author, Genre, Book>(
                    sql,
                    (book, author, genre) =>
                    {
                        if (resultBook == null) // Only initialize the first time
                        {
                            resultBook = book;
                            resultBook.Authors = new List<Author>();
                            resultBook.Genres = new List<Genre>();
                        }

                        if (author != null && !resultBook.Authors.Any(a => a.AuthorId == author.AuthorId))
                        {
                            resultBook.Authors.Add(author);
                        }

                        if (genre != null && !resultBook.Genres.Any(g => g.GenreId == genre.GenreId))
                        {
                            resultBook.Genres.Add(genre);
                        }

                        return resultBook;
                    },
                    new { BookId = id },
                    splitOn: "AuthorId,GenreId"
                );

                return resultBook; // Will be null if no book found
            }
        }
        
        public async Task<int> AddAsync(Book book)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    INSERT INTO Books (Title, PublicationYear, ISBN, PageCount, Summary, CoverImagePath, ReadingStatus, Rating, Notes)
                    VALUES (@Title, @PublicationYear, @ISBN, @PageCount, @Summary, @CoverImagePath, @ReadingStatus, @Rating, @Notes);
                    SELECT CAST(SCOPE_IDENTITY() as int)";


                return await connection.ExecuteScalarAsync<int>(sql, book);
            }
        }

        public async Task UpdateAsync(Book book)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    UPDATE Books SET
                        Title = @Title,
                        PublicationYear = @PublicationYear,
                        ISBN = @ISBN,
                        PageCount = @PageCount,
                        Summary = @Summary,
                        CoverImagePath = @CoverImagePath,
                        ReadingStatus = @ReadingStatus,
                        Rating = @Rating,
                        Notes = @Notes
                    WHERE BookId = @BookId";
                await connection.ExecuteAsync(sql, book);
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "DELETE FROM Books WHERE BookId = @BookId";
                await connection.ExecuteAsync(sql, new { BookId = id }); 
            }
        }

        public async Task AddBookAuthorAsync(int bookId, int authorId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookId, @AuthorId)";
                await connection.ExecuteAsync(sql, new { BookId = bookId, AuthorId = authorId }); // Use ExecuteAsync
            }
        }

        public async Task RemoveBookAuthorAsync(int bookId, int authorId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "DELETE FROM BookAuthors WHERE BookId = @BookId AND AuthorId = @AuthorId";
                await connection.ExecuteAsync(sql, new { BookId = bookId, AuthorId = authorId }); // Use ExecuteAsync
            }
        }

        public async Task<IEnumerable<Author>> GetAuthorsForBookAsync(int bookId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
            SELECT A.* FROM Authors A
            JOIN BookAuthors BA ON A.AuthorId = BA.AuthorId
            WHERE BA.BookId = @BookId";
                // Use QueryAsync and return Task<IEnumerable<Author>>
                return await connection.QueryAsync<Author>(sql, new { BookId = bookId });
            }
        }

        public async Task AddBookGenreAsync(int bookId, int genreId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @GenreId)";
                await connection.ExecuteAsync(sql, new { BookId = bookId, GenreId = genreId }); // Use ExecuteAsync
            }
        }

        public async Task RemoveBookGenreAsync(int bookId, int genreId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "DELETE FROM BookGenres WHERE BookId = @BookId AND GenreId = @GenreId";
                await connection.ExecuteAsync(sql, new { BookId = bookId, GenreId = genreId }); // Use ExecuteAsync
            }
        }   

        public async Task<IEnumerable<Genre>> GetGenresForBookAsync(int bookId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
            SELECT G.* FROM Genres G
            JOIN BookGenres BG ON G.GenreId = BG.GenreId
            WHERE BG.BookId = @BookId";
                // Use QueryAsync and return Task<IEnumerable<Genre>>
                return await connection.QueryAsync<Genre>(sql, new { BookId = bookId });
            }
        }

        public async Task<IEnumerable<Book>> SearchAsync(string searchTerm = null, string readingStatus = null, int? genreId = null) // Renamed to SearchAsync for consistency
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "SELECT B.* FROM Books B WHERE 1=1"; 

                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // We need to also search by author names here
                    sql += " AND (B.Title LIKE @SearchTerm OR B.Summary LIKE @SearchTerm OR B.ISBN LIKE @SearchTerm OR B.BookId IN (SELECT BA.BookId FROM BookAuthors BA JOIN Authors A ON BA.AuthorId = A.AuthorId WHERE A.FirstName LIKE @SearchTerm OR A.LastName LIKE @SearchTerm))";
                    parameters.Add("SearchTerm", "%" + searchTerm + "%");
                }

                if (!string.IsNullOrEmpty(readingStatus))
                {
                    sql += " AND B.ReadingStatus = @ReadingStatus";
                    parameters.Add("ReadingStatus", readingStatus);
                }

                if (genreId.HasValue && genreId.Value != 0) // Added check for GenreId != 0 (assuming 0 means "All")
                {
                    sql += " AND B.BookId IN (SELECT BG.BookId FROM BookGenres BG WHERE BG.GenreId = @GenreId)";
                    parameters.Add("GenreId", genreId.Value);
                }

                sql += " ORDER BY B.Title";

                // Use QueryAsync for asynchronous query
                return await connection.QueryAsync<Book>(sql, parameters);
            }
        }
        
        public Task SaveChangesAsync()
        {
            // In this Dapper setup, if a connection was opened for an Add/Update/Delete operation
            // (i.e., _connection is not null and is open), we close and dispose it here.
            // This acts as the "commit" point for the operations done on that connection.
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null; // Clear the connection reference
            }
            // Return a completed task since there's no asynchronous database operation happening directly in this method.
            return Task.CompletedTask;
        }
    }
}