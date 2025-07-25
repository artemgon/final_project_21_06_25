﻿using Domain.Entities;
using System.Collections.Generic;
using BookLibrary.Domain.Entities;

namespace DataAccess.Contracts
{
    public interface IGenreRepository
    {
        Task<IEnumerable<Genre>> GetAllAsync();
        Task<int> AddAsync(Genre entity);
        public Task<Genre> GetByIdAsync(int id);
        public Task UpdateAsync(Genre genre);
        public Task DeleteAsync(int id);
        Task SaveChangesAsync();
        Task ResetIdentitySeedAsync();
    }
}
