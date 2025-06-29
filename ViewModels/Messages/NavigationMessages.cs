// BookLibrary.ViewModels/Messages/NavigationMessages.cs
using CommunityToolkit.Mvvm.Messaging.Messages; // Required for ValueChangedMessage

namespace BookLibrary.ViewModels.Messages
{
    /// <summary>
    /// Message sent when a request to navigate to the Book Detail view for a new book is made.
    /// </summary>
    public class NavigateToAddBookMessage : ValueChangedMessage<bool>
    {
        // Value is not strictly needed for "add new", but ValueChangedMessage requires it.
        // We can just pass true to indicate a request.
        public NavigateToAddBookMessage() : base(true) { }
    }

    /// <summary>
    /// Message sent when a request to navigate to the Book Detail view for editing an existing book is made.
    /// Contains the ID of the book to edit.
    /// </summary>
    public class NavigateToEditBookMessage : ValueChangedMessage<int>
    {
        public NavigateToEditBookMessage(int bookId) : base(bookId) { }
    }

    /// <summary>
    /// Message sent when a request to navigate back to the Book List view is made.
    /// </summary>
    public class NavigateToBookListMessage : ValueChangedMessage<bool>
    {
        // Value is not strictly needed for navigation back, but ValueChangedMessage requires it.
        // We can just pass true to indicate a request.
        public NavigateToBookListMessage() : base(true) { }
    }
}