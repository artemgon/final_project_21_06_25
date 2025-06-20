using Dapper;
using BookLibrary.DataAccess.Contracts; // fixed namespace
using DataAccess.Database;
using Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BookLibrary.Domain.Entities;
using DataAccess.Contracts;

namespace DataAccess.Implementations
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly DbConnectionFactory _connectionFactory;
        private IDbConnection _connection;

        public AuthorRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Author>> GetAllAsync()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryAsync<Author>("SELECT * FROM Authors ORDER BY LastName, FirstName");
            }
        }

        public async Task<Author> GetByIdAsync(int id) // Changed return type to Task<Author> and added 'async'
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<Author>("SELECT * FROM Authors WHERE AuthorId = @Id", new { Id = id });
            }
        }

        public async Task<int> AddAsync(Author author)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    INSERT INTO Authors (FirstName, LastName, Biography)
                    VALUES (@FirstName, @LastName, @Biography);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
                return await connection.ExecuteScalarAsync<int>(sql, author);
            }
        }

        public async Task UpdateAsync(Author author)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    UPDATE Authors SET
                        FirstName = @FirstName,
                        LastName = @LastName,
                        Biography = @Biography
                    WHERE AuthorId = @AuthorId";
                await connection.ExecuteAsync(sql, author);
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.ExecuteAsync("DELETE FROM Authors WHERE AuthorId = @Id", new { Id = id });
            }
        }
        
        public Task SaveChangesAsync()
        {
            // This assumes _connection was opened by other methods in this repository
            // and should be closed/disposed when SaveChangesAsync is called.
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null; // Reset for next unit of work
            }
            return Task.CompletedTask; // Since no async DB operation is performed directly here
        }
    }
}