using System;
using KCL.Db.Entity;

namespace TestApp.Models
{
    [DbTable("articles")]
    public class Article : DbEntity<Article>
    {
        [DbKey("id", true, "s_articles")]
        public int Id { get; set; }

        [DbField("title")]
        public string Title { get; set; }

        [DbField("content")]
        public string Content { get; set; }

        [DbChildRelation("authors", "author_id", "id", "author")]
        public Author Author { get; set; }
    }
}