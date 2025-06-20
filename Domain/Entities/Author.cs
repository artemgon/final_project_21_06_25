using CommunityToolkit.Mvvm.ComponentModel; // For ObservableObject and ObservableProperty

namespace BookLibrary.Domain.Entities
{
    // Author should ideally be ObservableObject if its properties are directly bound and edited
    // and you want CanExecute to react to those live changes.
    public partial class Author : ObservableObject // Make it partial and inherit ObservableObject
    {
        public int AuthorId { get; set; }

        [ObservableProperty] // Use ObservableProperty for auto-generated property changed
        private string firstName;

        [ObservableProperty] // Use ObservableProperty for auto-generated property changed
        private string lastName;

        [ObservableProperty] // Use ObservableProperty for auto-generated property changed
        private string biography;
    }
}