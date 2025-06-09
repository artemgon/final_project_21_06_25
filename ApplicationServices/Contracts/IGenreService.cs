using Domain.Entities;
using System.Collections.Generic;

namespace ApplicationServices.Contracts
{
    public interface IGenreService
    {
        IEnumerable<Genre> GetAllGenres();
        Genre GetGenreById(int id);
        int CreateGenre(Genre genre);
        void UpdateGenre(Genre genre);
        void DeleteGenre(int id);
    }
}