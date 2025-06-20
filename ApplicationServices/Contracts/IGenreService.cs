using Domain.Entities;
using System.Collections.Generic;

namespace ApplicationServices.Contracts
{
    public interface IGenreService
    {
        Task<IEnumerable<Genre>> GetAllGenresAsync();
        Task AddGenreAsync(Genre genre);
        Task UpdateGenreAsync(Genre genre);
        Task DeleteGenreAsync(int genreId);
    }
}