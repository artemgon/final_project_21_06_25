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

-- Display current table contents
SELECT COUNT(*) as TotalAuthors FROM Authors
SELECT TOP 5 * FROM Authors

PRINT 'Database setup completed successfully!'
