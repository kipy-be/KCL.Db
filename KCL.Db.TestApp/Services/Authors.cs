using System;
using System.Collections.Generic;
using TestApp.Models;

namespace TestApp.Services
{
    public class AuthorsService : ServiceBase
    {
        public AuthorsService(DbInterface db)
            : base(db)
        {}

        public Author GetFromId(int id)
        {
            return _db.ParseOne<Author>
            (
                "SELECT " +
                    "a.id, " +
                    "a.nick, " +
                    "a.firstname, " +
                    "a.lastname " +
                "FROM authors a  " +
                "WHERE a.id = :p1",
                new Dictionary<string, object>()
                {
                    { "p1", id }
                }
            );
        }

        public List<Author> GetAll()
        {
            return _db.ParseMany<Author>
            (
                "SELECT " +
                    "a.id, " +
                    "a.nick, " +
                    "a.firstname, " +
                    "a.lastname " +
                "FROM authors a"
            );
        }
    }
}
