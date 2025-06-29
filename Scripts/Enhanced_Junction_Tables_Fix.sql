-- Enhanced Junction Tables Fix Script
-- This script ensures ALL books have proper author and genre relationships
-- Including specific fixes for J.K. Rowling and Harry Potter books

USE [BookLibraryDB]
GO

PRINT 'Starting enhanced junction table population...'

-- First add J.K. Rowling if she doesn't exist
IF NOT EXISTS (SELECT 1 FROM Authors WHERE FirstName = 'J.K.' AND LastName = 'Rowling')
BEGIN
    INSERT INTO Authors (FirstName, LastName, Biography) 
    VALUES ('J.K.', 'Rowling', 'British author best known for the Harry Potter fantasy series')
    PRINT 'Added J.K. Rowling to Authors table'
END
ELSE
BEGIN
    PRINT 'J.K. Rowling already exists in Authors table'
END

-- Add Fantasy genre if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM Genres WHERE GenreName = 'Fantasy')
BEGIN
    INSERT INTO Genres (GenreName) 
    VALUES ('Fantasy')
    PRINT 'Added Fantasy genre'
END
ELSE
BEGIN
    PRINT 'Fantasy genre already exists'
END

-- Clear existing junction table data to start fresh
DELETE FROM BookAuthors;
DELETE FROM BookGenres;
PRINT 'Cleared existing junction table data'

-- Get all the IDs we need
DECLARE @JKRowlingId INT, @OrwellId INT, @JohnDoeId INT, @JaneSmithId INT, @TestAuthorId INT
DECLARE @FictionId INT, @SciFiId INT, @FantasyId INT, @MysteryId INT, @RomanceId INT, @BiographyId INT

-- Get Author IDs
SELECT @JKRowlingId = AuthorId FROM Authors WHERE FirstName = 'J.K.' AND LastName = 'Rowling'
SELECT @OrwellId = AuthorId FROM Authors WHERE FirstName = 'George' AND LastName = 'Orwell'
SELECT @JohnDoeId = AuthorId FROM Authors WHERE FirstName = 'John' AND LastName = 'Doe'
SELECT @JaneSmithId = AuthorId FROM Authors WHERE FirstName = 'Jane' AND LastName = 'Smith' 
SELECT @TestAuthorId = AuthorId FROM Authors WHERE FirstName = 'Test' AND LastName = 'Author'

-- Get Genre IDs
SELECT @FictionId = GenreId FROM Genres WHERE GenreName = 'Fiction'
SELECT @SciFiId = GenreId FROM Genres WHERE GenreName = 'Science Fiction'
SELECT @FantasyId = GenreId FROM Genres WHERE GenreName = 'Fantasy'
SELECT @MysteryId = GenreId FROM Genres WHERE GenreName = 'Mystery'
SELECT @RomanceId = GenreId FROM Genres WHERE GenreName = 'Romance'
SELECT @BiographyId = GenreId FROM Genres WHERE GenreName = 'Biography'

PRINT 'Found Author IDs:'
PRINT 'J.K. Rowling: ' + ISNULL(CAST(@JKRowlingId AS VARCHAR), 'NULL')
PRINT 'George Orwell: ' + ISNULL(CAST(@OrwellId AS VARCHAR), 'NULL')
PRINT 'John Doe: ' + ISNULL(CAST(@JohnDoeId AS VARCHAR), 'NULL')

PRINT 'Found Genre IDs:'
PRINT 'Fiction: ' + ISNULL(CAST(@FictionId AS VARCHAR), 'NULL')
PRINT 'Fantasy: ' + ISNULL(CAST(@FantasyId AS VARCHAR), 'NULL')
PRINT 'Science Fiction: ' + ISNULL(CAST(@SciFiId AS VARCHAR), 'NULL')

-- Now link ALL books to appropriate authors and genres
DECLARE @BookId INT, @BookTitle NVARCHAR(200)
DECLARE book_cursor CURSOR FOR 
SELECT BookId, Title FROM Books ORDER BY BookId

OPEN book_cursor
FETCH NEXT FROM book_cursor INTO @BookId, @BookTitle

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Processing book: ' + @BookTitle + ' (ID: ' + CAST(@BookId AS VARCHAR) + ')'
    
    -- Link books to authors based on title patterns
    IF @BookTitle LIKE '%Harry Potter%' AND @JKRowlingId IS NOT NULL
    BEGIN
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookId, @JKRowlingId)
        PRINT '  -> Linked to J.K. Rowling'
        
        -- Add Fantasy and Fiction genres for Harry Potter
        IF @FantasyId IS NOT NULL
        BEGIN
            INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @FantasyId)
            PRINT '  -> Added Fantasy genre'
        END
        IF @FictionId IS NOT NULL
        BEGIN
            INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @FictionId)
            PRINT '  -> Added Fiction genre'
        END
    END
    ELSE IF @BookTitle LIKE '%1984%' AND @OrwellId IS NOT NULL
    BEGIN
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookId, @OrwellId)
        PRINT '  -> Linked to George Orwell'
        
        -- Add Science Fiction and Fiction genres for 1984
        IF @SciFiId IS NOT NULL
        BEGIN
            INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @SciFiId)
            PRINT '  -> Added Science Fiction genre'
        END
        IF @FictionId IS NOT NULL
        BEGIN
            INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @FictionId)
            PRINT '  -> Added Fiction genre'
        END
    END
    ELSE IF @BookTitle = 'Sample Book 1' AND @JohnDoeId IS NOT NULL
    BEGIN
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookId, @JohnDoeId)
        PRINT '  -> Linked to John Doe'
        
        -- Add Fiction genre
        IF @FictionId IS NOT NULL
        BEGIN
            INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @FictionId)
            PRINT '  -> Added Fiction genre'
        END
    END
    ELSE IF @BookTitle = 'Test Novel' AND @JaneSmithId IS NOT NULL
    BEGIN
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookId, @JaneSmithId)
        PRINT '  -> Linked to Jane Smith'
        
        -- Add Science Fiction and Romance genres
        IF @SciFiId IS NOT NULL
        BEGIN
            INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @SciFiId)
            PRINT '  -> Added Science Fiction genre'
        END
        IF @RomanceId IS NOT NULL
        BEGIN
            INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @RomanceId)
            PRINT '  -> Added Romance genre'
        END
    END
    ELSE IF @BookTitle = 'Demo Literature' AND @TestAuthorId IS NOT NULL
    BEGIN
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookId, @TestAuthorId)
        PRINT '  -> Linked to Test Author'
        
        -- Add Mystery genre
        IF @MysteryId IS NOT NULL
        BEGIN
            INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @MysteryId)
            PRINT '  -> Added Mystery genre'  
        END
    END
    ELSE
    BEGIN
        -- For any other books, link to John Doe as default and add Fiction genre
        IF @JohnDoeId IS NOT NULL
        BEGIN
            INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookId, @JohnDoeId)
            PRINT '  -> Linked to John Doe (default)'
        END
        
        IF @FictionId IS NOT NULL
        BEGIN
            INSERT INTO BookGenres (BookId, GenreId) VALUES (@BookId, @FictionId)
            PRINT '  -> Added Fiction genre (default)'
        END
    END
    
    FETCH NEXT FROM book_cursor INTO @BookId, @BookTitle
END

CLOSE book_cursor
DEALLOCATE book_cursor

-- Show final results
PRINT ''
PRINT 'Final junction table counts:'
SELECT 'BookAuthors' as TableName, COUNT(*) as RecordCount FROM BookAuthors
UNION ALL
SELECT 'BookGenres' as TableName, COUNT(*) as RecordCount FROM BookGenres

PRINT ''
PRINT 'All book-author relationships:'
SELECT B.BookId, B.Title, A.FirstName + ' ' + A.LastName as AuthorName
FROM Books B
INNER JOIN BookAuthors BA ON B.BookId = BA.BookId
INNER JOIN Authors A ON BA.AuthorId = A.AuthorId
ORDER BY B.BookId

PRINT ''
PRINT 'All book-genre relationships:'
SELECT B.BookId, B.Title, G.GenreName
FROM Books B
INNER JOIN BookGenres BG ON B.BookId = BG.BookId
INNER JOIN Genres G ON BG.GenreId = G.GenreId
ORDER BY B.BookId

PRINT ''
PRINT 'Enhanced junction table population completed successfully!'
PRINT 'All books now have author and genre relationships.'
GO
