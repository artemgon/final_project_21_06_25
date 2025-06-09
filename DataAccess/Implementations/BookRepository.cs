using BookLibrary.DataAccess.Contracts; 
using DataAccess.Database;
using Domain.Entities; 
using Dapper; 
using System.Collections.Generic; 
using System.Linq;

namespace BookLibrary.DataAccess.Implementations
{
    public class BookRepository : IBookRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public BookRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Book> GetAll()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "SELECT * FROM Books ORDER BY Title";
                return connection.Query<Book>(sql).ToList();
            }
        }

        public Book GetById(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "SELECT * FROM Books WHERE BookId = @BookId";
                return connection.QuerySingleOrDefault<Book>(sql, new { BookId = id }); 
            }
        }

        public int Add(Book book)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    INSERT INTO Books (Title, PublicationYear, ISBN, PageCount, Summary, CoverImagePath, ReadingStatus, Rating, Notes)
                    VALUES (@Title, @PublicationYear, @ISBN, @PageCount, @Summary, @CoverImagePath, @ReadingStatus, @Rating, @Notes);
                    SELECT CAST(SCOPE_IDENTITY() as int)";


                return connection.ExecuteScalar<int>(sql, book);
            }
        }

        public void Update(Book book)
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
                connection.Execute(sql, book);
            }
        }

        public void Delete(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "DELETE FROM Books WHERE BookId = @BookId";
                connection.Execute(sql, new { BookId = id }); 
            }
        }

        public void AddBookAuthor(int bookId, int authorId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookId, @AuthorId)";
                connection.Execute(sql, new { BookId = bookId, AuthorId = authorId });
            }
        }

        public void RemoveBookAuthor(int bookId, int authorId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "DELETE FROM BookAuthors WHERE BookId = @BookId AND AuthorId = @AuthorId";
                connection.Execute(sql, new { BookId = bookId, AuthorId = authorId });
            }
        }

        public IEnumerable<Author> GetAuthorsForBook(int bookId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    SELECT A.* FROM Authors A
                    JOIN BookAuthors BA ON A.AuthorId = BA.AuthorId
                    WHERE BA.BookId = @BookId";
                return connection.Query<Author>(sql, new { BookId = bookId }).ToList();
            }
        }

        public void AddBookGenre(int bookId, int genreId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @GenreId)";
                connection.Execute(sql, new { BookId = bookId, GenreId = genreId });
            }
        }

        public void RemoveBookGenre(int bookId, int genreId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "DELETE FROM BookGenres WHERE BookId = @BookId AND GenreId = @GenreId";
                connection.Execute(sql, new { BookId = bookId, GenreId = genreId });
            }
        }

        public IEnumerable<Genre> GetGenresForBook(int bookId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    SELECT G.* FROM Genres G
                    JOIN BookGenres BG ON G.GenreId = BG.GenreId
                    WHERE BG.BookId = @BookId";
                return connection.Query<Genre>(sql, new { BookId = bookId }).ToList();
            }
        }

        public IEnumerable<Book> SearchBooks(string searchTerm = null, string readingStatus = null, int? genreId = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "SELECT B.* FROM Books B WHERE 1=1"; 

                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    sql += " AND (B.Title LIKE @SearchTerm OR B.Summary LIKE @SearchTerm)";
                    parameters.Add("SearchTerm", "%" + searchTerm + "%");
                }

                if (!string.IsNullOrEmpty(readingStatus))
                {
                    sql += " AND B.ReadingStatus = @ReadingStatus";
                    parameters.Add("ReadingStatus", readingStatus);
                }

                if (genreId.HasValue)
                {
                    sql += " AND B.BookId IN (SELECT BG.BookId FROM BookGenres BG WHERE BG.GenreId = @GenreId)";
                    parameters.Add("GenreId", genreId.Value);
                }

                sql += " ORDER BY B.Title";

                return connection.Query<Book>(sql, parameters).ToList();
            }
        }
    }
}