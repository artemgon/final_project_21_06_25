using CommunityToolkit.Mvvm.ComponentModel; // IMPORTANT: Add this using statement

namespace BookLibrary.Domain.Entities
{
    public partial class Author : ObservableObject // IMPORTANT: Inherit from ObservableObject
    {
        public int AuthorId { get; set; } // No ObservableProperty needed for ID

        [ObservableProperty] // IMPORTANT: Add this attribute
        private string firstName = string.Empty;

        [ObservableProperty] // IMPORTANT: Add this attribute
        private string lastName = string.Empty;

        [ObservableProperty] // IMPORTANT: Add this attribute
        private string biography = string.Empty;
    }
}