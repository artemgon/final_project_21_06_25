using ApplicationServices.Contracts;
using Domain.Entities;
using System.Collections.ObjectModel;

namespace ViewModels
{
    public class AuthorManagerViewModel : ViewModelBase
    {
        private readonly IAuthorService _authorService;
        private ObservableCollection<Author> _authors;
        public ObservableCollection<Author> Authors
        {
            get => _authors;
            set => SetProperty(ref _authors, value);
        }

        private Author _selectedAuthor;
        public Author SelectedAuthor
        {
            get => _selectedAuthor;
            set => SetProperty(ref _selectedAuthor, value);
        }

        public AuthorManagerViewModel(IAuthorService authorService)
        {
            _authorService = authorService;
            Authors = new ObservableCollection<Author>();
            LoadAuthors(); // Load data on initialization
        }

        private void LoadAuthors()
        {
            Authors.Clear();
            foreach (var author in _authorService.GetAllAuthors())
            {
                Authors.Add(author);
            }
        }
        // Add Add/Edit/Delete commands later
    }
}