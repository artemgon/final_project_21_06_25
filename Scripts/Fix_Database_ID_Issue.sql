-- Database ID Fix Script
-- This script ensures that deleted book IDs are never reused
-- Run this script in SQL Server Management Studio

USE BookLibraryDB;
GO

-- First, let's check the current IDENTITY settings
DBCC CHECKIDENT('Books', NORESEED);
GO

-- Get the maximum BookId currently in the table
DECLARE @MaxId INT;
SELECT @MaxId = ISNULL(MAX(BookId), 0) FROM Books;

-- Reseed the identity to continue from the highest existing ID
DBCC CHECKIDENT('Books', RESEED, @MaxId);
GO

-- Do the same for other tables to prevent ID reuse
DBCC CHECKIDENT('Authors', NORESEED);
GO

DECLARE @MaxAuthorId INT;
SELECT @MaxAuthorId = ISNULL(MAX(AuthorId), 0) FROM Authors;
DBCC CHECKIDENT('Authors', RESEED, @MaxAuthorId);
GO

DBCC CHECKIDENT('Genres', NORESEED);
GO

DECLARE @MaxGenreId INT;
SELECT @MaxGenreId = ISNULL(MAX(GenreId), 0) FROM Genres;
DBCC CHECKIDENT('Genres', RESEED, @MaxGenreId);
GO

-- Optional: Add constraints to ensure proper ID behavior
-- This prevents manual insertion of IDs and ensures auto-increment works correctly

PRINT 'Database ID sequences have been fixed!';
PRINT 'Deleted IDs will no longer be reused.';
