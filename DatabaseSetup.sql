-- Database Setup Script for BookLibrary
-- Run this script in SQL Server Management Studio (SSMS) to ensure proper database setup

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BookLibraryDB')
BEGIN
    CREATE DATABASE [BookLibraryDB]
    PRINT 'Database BookLibraryDB created successfully'
END
ELSE
BEGIN
    PRINT 'Database BookLibraryDB already exists'
END

-- Use the database
USE [BookLibraryDB]
GO

-- Create Authors table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Authors' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Authors](
        [AuthorId] [int] IDENTITY(1,1) NOT NULL,
        [FirstName] [nvarchar](100) NOT NULL,
        [LastName] [nvarchar](100) NOT NULL,
        [Biography] [nvarchar](max) NULL,
        CONSTRAINT [PK_Authors] PRIMARY KEY CLUSTERED ([AuthorId] ASC)
    )
    PRINT 'Authors table created successfully'
END
ELSE
BEGIN
    PRINT 'Authors table already exists'
END

-- Create Genres table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Genres' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Genres](
        [GenreId] [int] IDENTITY(1,1) NOT NULL,
        [GenreName] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NULL,
        CONSTRAINT [PK_Genres] PRIMARY KEY CLUSTERED ([GenreId] ASC)
    )
    PRINT 'Genres table created successfully'
END
ELSE
BEGIN
    PRINT 'Genres table already exists'
END

-- Create Books table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Books' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Books](
        [BookId] [int] IDENTITY(1,1) NOT NULL,
        [Title] [nvarchar](200) NOT NULL,
        [PublicationYear] [int] NULL,
        [ISBN] [nvarchar](20) NULL,
        [PageCount] [int] NULL,
        [Summary] [nvarchar](max) NULL,
        [CoverImagePath] [nvarchar](500) NULL, -- This stores the path to the cover image file
        [ReadingStatus] [nvarchar](50) NULL,
        [Rating] [int] NULL,
        [Notes] [nvarchar](max) NULL,
        CONSTRAINT [PK_Books] PRIMARY KEY CLUSTERED ([BookId] ASC),
        CONSTRAINT [CHK_Rating] CHECK ([Rating] >= 1 AND [Rating] <= 5)
    )
    PRINT 'Books table created successfully'
END
ELSE
BEGIN
    PRINT 'Books table already exists'
END

-- Create BookAuthors junction table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BookAuthors' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[BookAuthors](
        [BookId] [int] NOT NULL,
        [AuthorId] [int] NOT NULL,
        CONSTRAINT [PK_BookAuthors] PRIMARY KEY CLUSTERED ([BookId] ASC, [AuthorId] ASC),
        CONSTRAINT [FK_BookAuthors_Books] FOREIGN KEY([BookId]) REFERENCES [dbo].[Books] ([BookId]) ON DELETE CASCADE,
        CONSTRAINT [FK_BookAuthors_Authors] FOREIGN KEY([AuthorId]) REFERENCES [dbo].[Authors] ([AuthorId]) ON DELETE CASCADE
    )
    PRINT 'BookAuthors table created successfully'
END
ELSE
BEGIN
    PRINT 'BookAuthors table already exists'
END

-- Create BookGenres junction table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BookGenres' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[BookGenres](
        [BookId] [int] NOT NULL,
        [GenreId] [int] NOT NULL,
        CONSTRAINT [PK_BookGenres] PRIMARY KEY CLUSTERED ([BookId] ASC, [GenreId] ASC),
        CONSTRAINT [FK_BookGenres_Books] FOREIGN KEY([BookId]) REFERENCES [dbo].[Books] ([BookId]) ON DELETE CASCADE,
        CONSTRAINT [FK_BookGenres_Genres] FOREIGN KEY([GenreId]) REFERENCES [dbo].[Genres] ([GenreId]) ON DELETE CASCADE
    )
    PRINT 'BookGenres table created successfully'
END
ELSE
BEGIN
    PRINT 'BookGenres table already exists'
END

-- Create Wishlist table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Wishlist' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Wishlist](
        [WishlistId] [int] IDENTITY(1,1) NOT NULL,
        [BookId] [int] NOT NULL,
        [DateAdded] [datetime] NOT NULL DEFAULT GETDATE(),
        [Priority] [int] NULL,
        [Notes] [nvarchar](500) NULL,
        CONSTRAINT [PK_Wishlist] PRIMARY KEY CLUSTERED ([WishlistId] ASC),
        CONSTRAINT [FK_Wishlist_Books] FOREIGN KEY([BookId]) REFERENCES [dbo].[Books] ([BookId]) ON DELETE CASCADE
    )
    PRINT 'Wishlist table created successfully'
END
ELSE
BEGIN
    PRINT 'Wishlist table already exists'
END

-- Grant permissions to current user (for Windows Authentication)
DECLARE @username NVARCHAR(128) = SYSTEM_USER
EXEC sp_addrolemember 'db_datareader', @username
EXEC sp_addrolemember 'db_datawriter', @username
PRINT 'Permissions granted to user: ' + @username

-- Insert some test data if table is empty
IF NOT EXISTS (SELECT * FROM Authors)
BEGIN
    INSERT INTO Authors (FirstName, LastName, Biography) VALUES 
    ('John', 'Doe', 'A sample author for testing purposes'),
    ('Jane', 'Smith', 'Another test author with a different background'),
    ('Test', 'Author', 'This is a test author created by the setup script')
    
    PRINT 'Test data inserted into Authors table'
END
ELSE
BEGIN
    PRINT 'Authors table already contains data'
END

-- Insert some test genres if table is empty
IF NOT EXISTS (SELECT * FROM Genres)
BEGIN
    INSERT INTO Genres (GenreName, Description) VALUES 
    ('Fiction', 'Literary works that are primarily imaginative'),
    ('Science Fiction', 'Fiction dealing with futuristic concepts'),
    ('Mystery', 'Fiction dealing with puzzling crimes'),
    ('Romance', 'Fiction focusing on romantic relationships'),
    ('Biography', 'Non-fiction accounts of real people''s lives')
    
    PRINT 'Test data inserted into Genres table'
END
ELSE
BEGIN
    PRINT 'Genres table already contains data'
END

-- Insert some test books if table is empty
IF NOT EXISTS (SELECT * FROM Books)
BEGIN
    INSERT INTO Books (Title, PublicationYear, ISBN, PageCount, Summary, ReadingStatus, Rating, Notes, CoverImagePath) VALUES 
    ('Sample Book 1', 2023, '978-1234567890', 300, 'This is a sample book for testing purposes', 'Completed', 4, 'Great read!', NULL),
    ('Test Novel', 2022, '978-0987654321', 250, 'Another test book with different properties', 'In Progress', 5, 'Amazing story', NULL),
    ('Demo Literature', 2021, '978-1122334455', 400, 'A longer book for testing pagination', 'Not Started', NULL, 'Waiting to read', NULL)
    
    PRINT 'Test data inserted into Books table'
END
ELSE
BEGIN
    PRINT 'Books table already contains data'
END

-- Display current table contents
SELECT COUNT(*) as TotalAuthors FROM Authors
SELECT TOP 5 * FROM Authors

PRINT 'Database setup completed successfully!'
GO
