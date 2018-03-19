using System;
using KCL.Db.Entity;

namespace TestApp.Models
{
    [DbTable("comments")]
    public class Comment : DbEntity<Comment>
    {
        [DbKey("id", true, "s_comment")]
        public int Id { get; set; }

        [DbField("content")]
        public string Content { get; set; }

        [DbChildRelation("author", "author_id", "id", "author")]
        public Author Author { get; set; }

        [DbParentRelation("article", "article_id", "id", "article")]
        public Article Article { get; set; }
    }
}