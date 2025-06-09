using ApplicationServices.Contracts;
using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.DataAccess.Contracts;
using Domain.Entities;
using System.Collections.Generic;

namespace BookLibrary.ApplicationServices.Implementations
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;

        public GenreService(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public IEnumerable<Genre> GetAllGenres()
        {
            return _genreRepository.GetAll();
        }

        public Genre GetGenreById(int id)
        {
            return _genreRepository.GetById(id);
        }

        public int CreateGenre(Genre genre)
        {
            return _genreRepository.Add(genre);
        }

        public void UpdateGenre(Genre genre)
        {
            _genreRepository.Update(genre);
        }

        public void DeleteGenre(int id)
        {
            _genreRepository.Delete(id);
        }
    }
}