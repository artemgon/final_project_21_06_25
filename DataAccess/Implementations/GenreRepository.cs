using BookLibrary.DataAccess.Contracts;
using Dapper;
using DataAccess.Database;
using Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BookLibrary.Domain.Entities;
using DataAccess.Contracts;

namespace DataAccess.Implementations
{
    public class GenreRepository : IGenreRepository
    {
        private readonly DbConnectionFactory _connectionFactory;
        private IDbConnection _connection;

        public GenreRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryAsync<Genre>("SELECT * FROM Genres ORDER BY GenreName"); ; // Changed to ToListAsync for better async handling
            }
        }

        // Make sure you have 'using System.Threading.Tasks;' at the top of your file.

        public async Task<Genre> GetByIdAsync(int id) // Changed return type to Task<Genre> and added 'async'
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                // Changed to QuerySingleOrDefaultAsync and added 'await'
                return await connection.QuerySingleOrDefaultAsync<Genre>("SELECT * FROM Genres WHERE GenreId = @Id", new { Id = id });
            }
        }

        public async Task<int> AddAsync(Genre genre)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    INSERT INTO Genres (GenreName)
                    VALUES (@GenreName);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
                return await connection.ExecuteScalarAsync<int>(sql, genre);
            }
        }

        public async Task UpdateAsync(Genre genre)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "UPDATE Genres SET GenreName = @GenreName WHERE GenreId = @GenreId";
                await connection.ExecuteAsync(sql, genre);
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.ExecuteAsync("DELETE FROM Genres WHERE GenreId = @Id", new { Id = id });
            }
        }
        
        public Task SaveChangesAsync()
        {
            // This is the "commit point" for operations performed using the shared _connection.
            // It closes and disposes of the connection, making any changes persistent.
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null; // Clear the reference to the disposed connection
            }
            // Return a completed task since no asynchronous database operation is happening directly here.
            return Task.CompletedTask;
        }

        public async Task ResetIdentitySeedAsync()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                // Reset the identity seed to 0, so the next insert will have ID 1
                await connection.ExecuteAsync("DBCC CHECKIDENT ('Genres', RESEED, 0)");
            }
        }
    }
}