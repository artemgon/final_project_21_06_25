-- SQL Script to Reset Identity Seeds
-- WARNING: Only use this in development/testing environments!
-- This can cause data integrity issues in production.

-- Reset Genre Identity
DBCC CHECKIDENT ('Genres', RESEED, 0);

-- Reset Author Identity  
DBCC CHECKIDENT ('Authors', RESEED, 0);

-- Reset Book Identity
DBCC CHECKIDENT ('Books', RESEED, 0);

-- Alternative method using ALTER TABLE (SQL Server 2012+)
-- ALTER TABLE Genres ALTER COLUMN GenreId RESTART WITH 1;
-- ALTER TABLE Authors ALTER COLUMN AuthorId RESTART WITH 1;
-- ALTER TABLE Books ALTER COLUMN BookId RESTART WITH 1;

-- To check current identity values:
-- DBCC CHECKIDENT ('Genres', NORESEED);
-- DBCC CHECKIDENT ('Authors', NORESEED);
-- DBCC CHECKIDENT ('Books', NORESEED);
