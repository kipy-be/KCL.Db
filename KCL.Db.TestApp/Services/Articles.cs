using System;
using System.Collections.Generic;
using TestApp.Models;

namespace TestApp.Services
{
    public class ArticlesService : ServiceBase
    {
        public ArticlesService(DbService db)
            : base(db)
        {}

        public Article GetFromId(int id)
        {
            return _db.DbInterface.ParseOne<Article>
            (
                "SELECT " +
                    "art.id, " +
                    "art.title, " +
                    "art.content, " +
                    "art.author_id, " +
                    "aut.nick AS author_nick " +
                "FROM articles art " +
                "INNER JOIN authors aut ON art.author_id = aut.id " +
                "WHERE art.id = :p1",
                new Dictionary<string, object>()
                {
                    { "p1", id }
                }
            );
        }
    }
}
