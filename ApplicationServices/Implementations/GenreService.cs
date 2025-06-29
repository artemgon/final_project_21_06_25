using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.DataAccess.Contracts; // Assuming your repository interface is here
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationServices.Contracts;
using BookLibrary.Domain.Entities;
using DataAccess.Contracts;
using System.Linq; // Add this for the .Any() method

namespace BookLibrary.ApplicationServices.Implementations
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;

        public GenreService(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public async Task<IEnumerable<Genre>> GetAllGenresAsync()
        {
            return await _genreRepository.GetAllAsync(); // Ensure your repository method is also async
        }

        public async Task AddGenreAsync(Genre genre)
        {
            await _genreRepository.AddAsync(genre);
            await _genreRepository.SaveChangesAsync(); // Commit changes to DB
        }

        public async Task UpdateGenreAsync(Genre genre)
        {
            await _genreRepository.UpdateAsync(genre); // Assuming this marks the entity as modified
            await _genreRepository.SaveChangesAsync();
        }

        public async Task DeleteGenreAsync(int genreId)
        {
            await _genreRepository.DeleteAsync(genreId);
            await _genreRepository.SaveChangesAsync();
            
            // Check if there are any genres left
            var remainingGenres = await _genreRepository.GetAllAsync();
            if (!remainingGenres.Any())
            {
                // Reset identity seed to 0 so next insert will be ID 1
                await _genreRepository.ResetIdentitySeedAsync();
            }
        }
    }
}