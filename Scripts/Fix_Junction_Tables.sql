-- Fix Junction Tables Script
-- This script populates the BookAuthors and BookGenres junction tables
-- to establish proper relationships between books, authors, and genres

USE [BookLibraryDB]
GO

PRINT 'Starting junction table population...'

-- First, let's see what we're working with
PRINT 'Current state:'
SELECT 'Books' as TableName, COUNT(*) as RecordCount FROM Books
UNION ALL
SELECT 'Authors' as TableName, COUNT(*) as RecordCount FROM Authors
UNION ALL
SELECT 'Genres' as TableName, COUNT(*) as RecordCount FROM Genres
UNION ALL
SELECT 'BookAuthors' as TableName, COUNT(*) as RecordCount FROM BookAuthors
UNION ALL
SELECT 'BookGenres' as TableName, COUNT(*) as RecordCount FROM BookGenres

-- Clear existing junction table data to start fresh
DELETE FROM BookAuthors;
DELETE FROM BookGenres;
PRINT 'Cleared existing junction table data'

-- Get the IDs we'll need
DECLARE @Book1Id INT, @Book2Id INT, @Book3Id INT
DECLARE @Author1Id INT, @Author2Id INT, @Author3Id INT, @OrwellId INT
DECLARE @FictionId INT, @SciFiId INT, @MysteryId INT, @BiographyId INT, @RomanceId INT

-- Get Book IDs (from the sample data)
SELECT @Book1Id = BookId FROM Books WHERE Title = 'Sample Book 1'
SELECT @Book2Id = BookId FROM Books WHERE Title = 'Test Novel'  
SELECT @Book3Id = BookId FROM Books WHERE Title = 'Demo Literature'

-- Get Author IDs (from sample data + any George Orwell if exists)
SELECT @Author1Id = AuthorId FROM Authors WHERE FirstName = 'John' AND LastName = 'Doe'
SELECT @Author2Id = AuthorId FROM Authors WHERE FirstName = 'Jane' AND LastName = 'Smith'
SELECT @Author3Id = AuthorId FROM Authors WHERE FirstName = 'Test' AND LastName = 'Author'
SELECT @OrwellId = AuthorId FROM Authors WHERE FirstName = 'George' AND LastName = 'Orwell'

-- Get Genre IDs
SELECT @FictionId = GenreId FROM Genres WHERE GenreName = 'Fiction'
SELECT @SciFiId = GenreId FROM Genres WHERE GenreName = 'Science Fiction'
SELECT @MysteryId = GenreId FROM Genres WHERE GenreName = 'Mystery'
SELECT @BiographyId = GenreId FROM Genres WHERE GenreName = 'Biography'
SELECT @RomanceId = GenreId FROM Genres WHERE GenreName = 'Romance'

-- Populate BookAuthors junction table
PRINT 'Populating BookAuthors...'

-- Link Sample Book 1 to John Doe
IF @Book1Id IS NOT NULL AND @Author1Id IS NOT NULL
BEGIN
    INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@Book1Id, @Author1Id)
    PRINT 'Linked Sample Book 1 to John Doe'
END

-- Link Test Novel to Jane Smith
IF @Book2Id IS NOT NULL AND @Author2Id IS NOT NULL
BEGIN
    INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@Book2Id, @Author2Id)
    PRINT 'Linked Test Novel to Jane Smith'
END

-- Link Demo Literature to Test Author
IF @Book3Id IS NOT NULL AND @Author3Id IS NOT NULL
BEGIN
    INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@Book3Id, @Author3Id)
    PRINT 'Linked Demo Literature to Test Author'
END

-- If 1984 exists and George Orwell exists, link them
DECLARE @Book1984Id INT
SELECT @Book1984Id = BookId FROM Books WHERE Title LIKE '%1984%'
IF @Book1984Id IS NOT NULL AND @OrwellId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM BookAuthors WHERE BookId = @Book1984Id AND AuthorId = @OrwellId)
    BEGIN
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@Book1984Id, @OrwellId)
        PRINT 'Linked 1984 to George Orwell'
    END
    ELSE
    BEGIN
        PRINT '1984 is already linked to George Orwell'
    END
END

-- Populate BookGenres junction table
PRINT 'Populating BookGenres...'

-- Link Sample Book 1 to Fiction
IF @Book1Id IS NOT NULL AND @FictionId IS NOT NULL
BEGIN
    INSERT INTO BookGenres (BookId, GenreId) VALUES (@Book1Id, @FictionId)
    PRINT 'Linked Sample Book 1 to Fiction genre'
END

-- Link Test Novel to Science Fiction and Romance (example of multiple genres)
IF @Book2Id IS NOT NULL AND @SciFiId IS NOT NULL
BEGIN
    INSERT INTO BookGenres (BookId, GenreId) VALUES (@Book2Id, @SciFiId)
    PRINT 'Linked Test Novel to Science Fiction genre'
END

IF @Book2Id IS NOT NULL AND @RomanceId IS NOT NULL
BEGIN
    INSERT INTO BookGenres (BookId, GenreId) VALUES (@Book2Id, @RomanceId)
    PRINT 'Linked Test Novel to Romance genre'
END

-- Link Demo Literature to Mystery
IF @Book3Id IS NOT NULL AND @MysteryId IS NOT NULL
BEGIN
    INSERT INTO BookGenres (BookId, GenreId) VALUES (@Book3Id, @MysteryId)
    PRINT 'Linked Demo Literature to Mystery genre'
END

-- If 1984 exists, link it to Science Fiction
IF @Book1984Id IS NOT NULL AND @SciFiId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM BookGenres WHERE BookId = @Book1984Id AND GenreId = @SciFiId)
    BEGIN
        INSERT INTO BookGenres (BookId, GenreId) VALUES (@Book1984Id, @SciFiId)
        PRINT 'Linked 1984 to Science Fiction genre'
    END
END

-- Add Fiction genre to 1984 as well (it's both sci-fi and fiction)
IF @Book1984Id IS NOT NULL AND @FictionId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM BookGenres WHERE BookId = @Book1984Id AND GenreId = @FictionId)
    BEGIN
        INSERT INTO BookGenres (BookId, GenreId) VALUES (@Book1984Id, @FictionId)
        PRINT 'Linked 1984 to Fiction genre'
    END
END

-- Show final state
PRINT 'Final junction table counts:'
SELECT 'BookAuthors' as TableName, COUNT(*) as RecordCount FROM BookAuthors
UNION ALL
SELECT 'BookGenres' as TableName, COUNT(*) as RecordCount FROM BookGenres

-- Show a sample of the relationships created
PRINT 'Sample book-author relationships:'
SELECT TOP 10 B.Title, A.FirstName + ' ' + A.LastName as AuthorName
FROM Books B
INNER JOIN BookAuthors BA ON B.BookId = BA.BookId
INNER JOIN Authors A ON BA.AuthorId = A.AuthorId
ORDER BY B.Title

PRINT 'Sample book-genre relationships:'
SELECT TOP 10 B.Title, G.GenreName
FROM Books B
INNER JOIN BookGenres BG ON B.BookId = BG.BookId
INNER JOIN Genres G ON BG.GenreId = G.GenreId
ORDER BY B.Title

PRINT 'Junction table population completed successfully!'
GO
