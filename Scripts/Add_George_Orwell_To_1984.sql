-- Script to add George Orwell as an author and link him to the 1984 book
-- Run this in SQL Server Management Studio

USE [BookLibraryDB]
GO

-- First, check if George Orwell already exists
IF NOT EXISTS (SELECT 1 FROM Authors WHERE FirstName = 'George' AND LastName = 'Orwell')
BEGIN
    -- Insert George Orwell as an author
    INSERT INTO Authors (FirstName, LastName, Biography)
    VALUES ('George', 'Orwell', 'British author and journalist known for his dystopian fiction including 1984 and Animal Farm.')
    
    PRINT 'George Orwell added to Authors table'
END
ELSE
BEGIN
    PRINT 'George Orwell already exists in Authors table'
END

-- Get the AuthorId for George Orwell
DECLARE @OrwellAuthorId INT
SELECT @OrwellAuthorId = AuthorId FROM Authors WHERE FirstName = 'George' AND LastName = 'Orwell'

-- Check if 1984 book exists and get its BookId
DECLARE @BookId1984 INT
SELECT @BookId1984 = BookId FROM Books WHERE Title LIKE '%1984%'

IF @BookId1984 IS NOT NULL AND @OrwellAuthorId IS NOT NULL
BEGIN
    -- Check if the relationship already exists
    IF NOT EXISTS (SELECT 1 FROM BookAuthors WHERE BookId = @BookId1984 AND AuthorId = @OrwellAuthorId)
    BEGIN
        -- Link George Orwell to 1984
        INSERT INTO BookAuthors (BookId, AuthorId)
        VALUES (@BookId1984, @OrwellAuthorId)
        
        PRINT '1984 has been linked to George Orwell'
    END
    ELSE
    BEGIN
        PRINT '1984 is already linked to George Orwell'
    END
END
ELSE
BEGIN
    IF @BookId1984 IS NULL
        PRINT 'ERROR: 1984 book not found in database'
    IF @OrwellAuthorId IS NULL
        PRINT 'ERROR: George Orwell not found in Authors table'
END

-- Show results
SELECT 
    B.BookId,
    B.Title,
    A.FirstName + ' ' + A.LastName AS AuthorName
FROM Books B
LEFT JOIN BookAuthors BA ON B.BookId = BA.BookId
LEFT JOIN Authors A ON BA.AuthorId = A.AuthorId
WHERE B.Title LIKE '%1984%'

PRINT 'Script completed - check the results above'
