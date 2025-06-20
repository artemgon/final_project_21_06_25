using BookLibrary.DataAccess.Contracts;
using Dapper;
using DataAccess.Database;
using Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataAccess.Contracts;

namespace DataAccess.Implementations
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly DbConnectionFactory _connectionFactory;
        private IDbConnection _connection;

        public WishlistRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<WishlistItem>> GetAllAsync()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryAsync<WishlistItem>("SELECT * FROM Wishlist ORDER BY DateAdded DESC");
            }
        }

        // Make sure you have 'using System.Threading.Tasks;' at the top of your file.

        public async Task<WishlistItem> GetByIdAsync(int id) // Changed return type to Task<WishlistItem> and added 'async'
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                // Changed to QuerySingleOrDefaultAsync and added 'await'
                return await connection.QuerySingleOrDefaultAsync<WishlistItem>("SELECT * FROM WishlistItems WHERE WishlistItemId = @Id", new { Id = id });
            }
        }

       public async Task<int> AddAsync(WishlistItem item)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    INSERT INTO Wishlist (Title, Author, Notes)
                    VALUES (@Title, @Author, @Notes);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
                return await connection.ExecuteScalarAsync<int>(sql, item);
            }
        }

        public async Task UpdateAsync(WishlistItem item)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    UPDATE Wishlist SET
                        Title = @Title,
                        Author = @Author,
                        Notes = @Notes
                    WHERE WishlistItemId = @WishlistItemId";
                await connection.ExecuteAsync(sql, item);
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.ExecuteAsync("DELETE FROM Wishlist WHERE WishlistItemId = @Id", new { Id = id });
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
    }
}