using BookLibrary.DataAccess.Contracts;
using Dapper;
using DataAccess.Database;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace BookLibrary.DataAccess.Implementations
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public WishlistRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<WishlistItem> GetAll()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return connection.Query<WishlistItem>("SELECT * FROM Wishlist ORDER BY DateAdded DESC").ToList();
            }
        }

        public WishlistItem GetById(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return connection.QuerySingleOrDefault<WishlistItem>("SELECT * FROM Wishlist WHERE WishlistItemId = @Id", new { Id = id });
            }
        }

        public int Add(WishlistItem item)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    INSERT INTO Wishlist (Title, Author, Notes)
                    VALUES (@Title, @Author, @Notes);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
                return connection.ExecuteScalar<int>(sql, item);
            }
        }

        public void Update(WishlistItem item)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    UPDATE Wishlist SET
                        Title = @Title,
                        Author = @Author,
                        Notes = @Notes
                    WHERE WishlistItemId = @WishlistItemId";
                connection.Execute(sql, item);
            }
        }

        public void Delete(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Execute("DELETE FROM Wishlist WHERE WishlistItemId = @Id", new { Id = id });
            }
        }
    }
}