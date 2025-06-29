-- Script to fix ID gaps in Books table and reset auto-increment
-- This will renumber all books sequentially starting from 1

USE [BookLibraryDB]
GO

-- Step 1: Create a temporary table with new sequential IDs
CREATE TABLE #TempBooks (
    OldBookId INT,
    NewBookId INT IDENTITY(1,1),
    Title NVARCHAR(200),
    PublicationYear INT,
    ISBN NVARCHAR(20),
    PageCount INT,
    Summary NVARCHAR(MAX),
    CoverImagePath NVARCHAR(500),
    ReadingStatus NVARCHAR(50),
    Rating INT,
    Notes NVARCHAR(MAX)
)

-- Step 2: Insert all books with new sequential IDs
INSERT INTO #TempBooks (OldBookId, Title, PublicationYear, ISBN, PageCount, Summary, CoverImagePath, ReadingStatus, Rating, Notes)
SELECT BookId, Title, PublicationYear, ISBN, PageCount, Summary, CoverImagePath, ReadingStatus, Rating, Notes
FROM Books
ORDER BY BookId

-- Step 3: Update junction tables with new IDs
-- Update BookAuthors
UPDATE BA
SET BA.BookId = TB.NewBookId
FROM BookAuthors BA
INNER JOIN #TempBooks TB ON BA.BookId = TB.OldBookId

-- Update BookGenres
UPDATE BG
SET BG.BookId = TB.NewBookId
FROM BookGenres BG
INNER JOIN #TempBooks TB ON BG.BookId = TB.OldBookId

-- Step 4: Clear the Books table
DELETE FROM Books

-- Step 5: Reset the identity seed
DBCC CHECKIDENT('Books', RESEED, 0)

-- Step 6: Insert books back with sequential IDs
SET IDENTITY_INSERT Books ON

INSERT INTO Books (BookId, Title, PublicationYear, ISBN, PageCount, Summary, CoverImagePath, ReadingStatus, Rating, Notes)
SELECT NewBookId, Title, PublicationYear, ISBN, PageCount, Summary, CoverImagePath, ReadingStatus, Rating, Notes
FROM #TempBooks
ORDER BY NewBookId

SET IDENTITY_INSERT Books OFF

-- Step 7: Clean up
DROP TABLE #TempBooks

-- Step 8: Reset identity to continue from the highest ID + 1
DECLARE @MaxId INT 
SELECT @MaxId = ISNULL(MAX(BookId), 0) FROM Books
DBCC CHECKIDENT('Books', RESEED, @MaxId)

-- Step 9: Print results using variables
DECLARE @TotalBooks INT
DECLARE @MinId INT
DECLARE @MaxIdFinal INT

SELECT @TotalBooks = COUNT(*) FROM Books
SELECT @MinId = ISNULL(MIN(BookId), 0) FROM Books  
SELECT @MaxIdFinal = ISNULL(MAX(BookId), 0) FROM Books

PRINT 'Book IDs have been reset to sequential values starting from 1'
PRINT 'Total books: ' + CAST(@TotalBooks AS VARCHAR(10))
PRINT 'ID range: ' + CAST(@MinId AS VARCHAR(10)) + ' to ' + CAST(@MaxIdFinal AS VARCHAR(10))
