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
            try
            {
                System.Diagnostics.Debug.WriteLine("AuthorRepository.GetAllAsync: Starting database query...");
                using (var connection = _connectionFactory.CreateConnection())
                {
                    // Test database connection first
                    System.Diagnostics.Debug.WriteLine($"AuthorRepository.GetAllAsync: Connection String: {connection.ConnectionString}");
                    System.Diagnostics.Debug.WriteLine($"AuthorRepository.GetAllAsync: Connection State: {connection.State}");
                    
                    // Try to open connection explicitly
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine($"AuthorRepository.GetAllAsync: Connection opened successfully. State: {connection.State}");
                    
                    // Test if database and table exist
                    var tableExistsQuery = @"
                        SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = 'Authors'";
                    
                    var tableExists = await connection.QuerySingleAsync<int>(tableExistsQuery);
                    System.Diagnostics.Debug.WriteLine($"AuthorRepository.GetAllAsync: Authors table exists: {tableExists > 0}");
                    
                    if (tableExists == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("AuthorRepository.GetAllAsync: AUTHORS TABLE DOES NOT EXIST!");
                        throw new InvalidOperationException("The 'Authors' table does not exist in the database. Please check your database schema.");
                    }
                    
                    // Check table structure
                    var columnsQuery = @"
                        SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Authors' 
                        ORDER BY ORDINAL_POSITION";
                    
                    var columns = await connection.QueryAsync<string>(columnsQuery);
                    System.Diagnostics.Debug.WriteLine($"AuthorRepository.GetAllAsync: Table columns: {string.Join(", ", columns)}");
                    
                    // Count total rows in table
                    var rowCount = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Authors");
                    System.Diagnostics.Debug.WriteLine($"AuthorRepository.GetAllAsync: Total rows in Authors table: {rowCount}");
                    
                    // Now execute the actual query
                    var sql = "SELECT * FROM Authors ORDER BY LastName, FirstName";
                    System.Diagnostics.Debug.WriteLine($"AuthorRepository.GetAllAsync: Executing SQL: {sql}");
                    
                    var authors = await connection.QueryAsync<Author>(sql);
                    var authorsList = authors?.ToList() ?? new List<Author>();
                    
                    System.Diagnostics.Debug.WriteLine($"AuthorRepository.GetAllAsync: Query returned {authorsList.Count} authors");
                    
                    foreach (var author in authorsList)
                    {
                        System.Diagnostics.Debug.WriteLine($"  -> Author: ID={author.AuthorId}, Name={author.FirstName} {author.LastName}");
                    }
                    
                    return authorsList;
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"AuthorRepository.GetAllAsync SQL ERROR: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"SQL Error Number: {sqlEx.Number}");
                System.Diagnostics.Debug.WriteLine($"SQL Error Severity: {sqlEx.Class}");
                System.Diagnostics.Debug.WriteLine($"SQL Error State: {sqlEx.State}");
                
                if (sqlEx.Number == 2) // Cannot open database
                {
                    throw new InvalidOperationException("Cannot connect to the database. Please check if SQL Server is running and the database exists.", sqlEx);
                }
                else if (sqlEx.Number == 18456) // Login failed
                {
                    throw new InvalidOperationException("Database login failed. Please check your Windows authentication permissions.", sqlEx);
                }
                else if (sqlEx.Number == 208) // Invalid object name
                {
                    throw new InvalidOperationException("The 'Authors' table does not exist. Please check your database schema.", sqlEx);
                }
                
                throw new InvalidOperationException($"Database error: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuthorRepository.GetAllAsync ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
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
            try
            {
                System.Diagnostics.Debug.WriteLine($"AuthorRepository.AddAsync: Adding author {author.FirstName} {author.LastName}");
                using (var connection = _connectionFactory.CreateConnection())
                {
                    var sql = @"
                        INSERT INTO Authors (FirstName, LastName, Biography)
                        VALUES (@FirstName, @LastName, @Biography);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
                    
                    System.Diagnostics.Debug.WriteLine($"AuthorRepository.AddAsync: Executing SQL: {sql}");
                    var newId = await connection.ExecuteScalarAsync<int>(sql, author);
                    System.Diagnostics.Debug.WriteLine($"AuthorRepository.AddAsync: New ID returned: {newId}");
                    return newId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuthorRepository.AddAsync ERROR: {ex.Message}");
                throw;
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

        public async Task ResetIdentitySeedAsync()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                // Reset the identity seed to 0, so the next insert will have ID 1
                await connection.ExecuteAsync("DBCC CHECKIDENT ('Authors', RESEED, 0)");
            }
        }
    }
}