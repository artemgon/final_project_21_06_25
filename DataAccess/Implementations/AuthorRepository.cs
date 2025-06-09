using Dapper;
using DataAccess.Contracts;
using DataAccess.Database;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace BookLibrary.DataAccess.Implementations
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public AuthorRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Author> GetAll()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return connection.Query<Author>("SELECT * FROM Authors ORDER BY LastName, FirstName").ToList();
            }
        }

        public Author GetById(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return connection.QuerySingleOrDefault<Author>("SELECT * FROM Authors WHERE AuthorId = @Id", new { Id = id });
            }
        }

        public int Add(Author author)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    INSERT INTO Authors (FirstName, LastName, Biography)
                    VALUES (@FirstName, @LastName, @Biography);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
                return connection.ExecuteScalar<int>(sql, author);
            }
        }

        public void Update(Author author)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    UPDATE Authors SET
                        FirstName = @FirstName,
                        LastName = @LastName,
                        Biography = @Biography
                    WHERE AuthorId = @AuthorId";
                connection.Execute(sql, author);
            }
        }

        public void Delete(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Execute("DELETE FROM Authors WHERE AuthorId = @Id", new { Id = id });
            }
        }
    }
}