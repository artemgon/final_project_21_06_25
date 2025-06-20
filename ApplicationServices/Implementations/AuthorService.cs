using ApplicationServices.Contracts;
using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.DataAccess.Contracts;
using DataAccess.Contracts;
using Domain.Entities;
using Dapper;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationServices.Implementations
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;

        private readonly IDbConnection _dbConnection;
        
        public AuthorService(IAuthorRepository authorRepository, IDbConnection dbConnection)
        {
            _authorRepository = authorRepository;
            _dbConnection = dbConnection;
        }

        public IEnumerable<Author> GetAllAuthors()
        {
            var sql = "SELECT * FROM Authors";
            return _dbConnection.Query<Author>(sql);
        }

        public Author GetAuthorById(int id)
        {
            var sql = "SELECT * FROM Authors WHERE AuthorId = @AuthorId";
            return _dbConnection.Query<Author>(sql, new { AuthorId = id }).SingleOrDefault();
        }

        public int CreateAuthor(Author author)
        {
            var sql = "INSERT INTO Authors (FirstName, LastName, Biography) VALUES (@FirstName, @LastName, @Biography); SELECT CAST(SCOPE_IDENTITY() as int);";
            return _dbConnection.Query<int>(sql, new
            {
                author.FirstName,
                author.LastName,
                author.Biography
            }).Single();
        }

        public void UpdateAuthor(Author author)
        {
            var sql = "UPDATE Authors SET FirstName = @FirstName, LastName = @LastName, Biography = @Biography WHERE AuthorId = @AuthorId";
            _dbConnection.Execute(sql, new
            {
                author.FirstName,
                author.LastName,
                author.Biography,
                AuthorId = author.AuthorId
            });
        }

        public void DeleteAuthor(int id)
        {
            var sql = "DELETE FROM Authors WHERE AuthorId = @AuthorId";
            _dbConnection.Execute(sql, new { AuthorId = id });
        }
    }
}