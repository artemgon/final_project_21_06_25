using ApplicationServices.Contracts;
using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.DataAccess.Contracts;
using DataAccess.Contracts;
using Domain.Entities;
using System.Collections.Generic;

namespace BookLibrary.ApplicationServices.Implementations
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;

        public AuthorService(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        public IEnumerable<Author> GetAllAuthors()
        {
            return _authorRepository.GetAll();
        }

        public Author GetAuthorById(int id)
        {
            return _authorRepository.GetById(id);
        }

        public int CreateAuthor(Author author)
        {
            return _authorRepository.Add(author);
        }

        public void UpdateAuthor(Author author)
        {
           _authorRepository.Update(author);
        }

        public void DeleteAuthor(int id)
        {
            _authorRepository.Delete(id);
        }
    }
}