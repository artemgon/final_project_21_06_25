# BookLibrary - Personal Book Management System

A desktop application built with WPF and C# for managing your personal book collection. Track your reading progress, organize books by genres and authors, and maintain detailed information about each book including cover images.

## Features

- **Book Management**: Add, edit, and delete books from your collection
- **Author & Genre Organization**: Categorize books by authors and genres
- **Reading Progress Tracking**: Monitor your reading status and take notes
- **Cover Image Support**: Store and display book cover images
- **Rating System**: Rate books on a 5-star scale
- **Advanced Search**: Find books by title, author, genre, or reading status
- **Reading Statistics**: Track your reading habits and progress

## Technology Stack

- **Frontend**: WPF (Windows Presentation Foundation)
- **Backend**: C# .NET
- **Database**: SQLite
- **Architecture**: MVVM (Model-View-ViewModel)
- **IDE**: JetBrains Rider

## Database Schema

The application uses SQLite with the following main tables:
- `Books` - Core book information including cover image paths
- `Authors` - Author details
- `Genres` - Book categories
- `BookAuthors` - Many-to-many relationship between books and authors
- `BookGenres` - Many-to-many relationship between books and genres

## Key Components
ImageService

Handles book cover image operations:


- Saves images to local directory
- Generates unique filenames
- Manages image deletion
- Provides file selection dialog

BookRepository

Manages database operations for books:


- CRUD operations
- Search functionality
- Relationship management

ViewModels

Implement MVVM pattern:


- BookDetailViewModel - Book details and editing
- AddBookViewModel - New book creation
- Data binding and command handling
