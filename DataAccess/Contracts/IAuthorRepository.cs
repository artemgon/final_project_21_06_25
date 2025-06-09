using Domain.Entities;
using System.Collections.Generic;

namespace DataAccess.Contracts
{
    public interface IAuthorRepository
    {
        IEnumerable<Author> GetAll();
        Author GetById(int id);
        int Add(Author author);
        void Update(Author author);
        void Delete(int id);
    }
}