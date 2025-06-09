using Domain.Entities;
using System.Collections.Generic;

namespace BookLibrary.DataAccess.Contracts
{
    public interface IGenreRepository
    {
        IEnumerable<Genre> GetAll();
        Genre GetById(int id);
        int Add(Genre genre);
        void Update(Genre genre);
        void Delete(int id);
    }
}
