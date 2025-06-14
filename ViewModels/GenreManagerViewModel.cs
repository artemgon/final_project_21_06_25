using ApplicationServices.Contracts;
using BookLibrary.ApplicationServices.Contracts;
using Domain.Entities;
using System.Collections.ObjectModel;

namespace ViewModels
{
    public class GenreManagerViewModel : ViewModelBase
    {
        private readonly IGenreService _genreService;
        private ObservableCollection<Genre> _genres;
        public ObservableCollection<Genre> Genres
        {
            get => _genres;
            set => SetProperty(ref _genres, value);
        }

        private Genre _selectedGenre;
        public Genre SelectedGenre
        {
            get => _selectedGenre;
            set => SetProperty(ref _selectedGenre, value);
        }

        public GenreManagerViewModel(IGenreService genreService)
        {
            _genreService = genreService;
            Genres = new ObservableCollection<Genre>();
            LoadGenres(); // Load data on initialization
        }

        private void LoadGenres()
        {
            Genres.Clear();
            foreach (var genre in _genreService.GetAllGenres())
            {
                Genres.Add(genre);
            }
        }
        // Add Add/Edit/Delete commands later
    }
}