using BookLibrary.DataAccess.Contracts;
using Dapper;
using DataAccess.Database;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace BookLibrary.DataAccess.Implementations
{
    public class GenreRepository : IGenreRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public GenreRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Genre> GetAll()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return connection.Query<Genre>("SELECT * FROM Genres ORDER BY GenreName").ToList();
            }
        }

        public Genre GetById(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                return connection.QuerySingleOrDefault<Genre>("SELECT * FROM Genres WHERE GenreId = @Id", new { Id = id });
            }
        }

        public int Add(Genre genre)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = @"
                    INSERT INTO Genres (GenreName)
                    VALUES (@GenreName);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
                return connection.ExecuteScalar<int>(sql, genre);
            }
        }

        public void Update(Genre genre)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                var sql = "UPDATE Genres SET GenreName = @GenreName WHERE GenreId = @GenreId";
                connection.Execute(sql, genre);
            }
        }

        public void Delete(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Execute("DELETE FROM Genres WHERE GenreId = @Id", new { Id = id });
            }
        }
    }
}