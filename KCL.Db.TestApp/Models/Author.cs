using System;
using KCL.Db.Entity;

namespace TestApp.Models
{
    [DbTable("authors")]
    public class Author : DbEntity<Author>
    {
        [DbKey("id", true, "s_authors")]
        public int Id { get; set; }

        [DbField("nick")]
        public string Nick { get; set; }

        [DbField("firstname")]
        public string FirstName { get; set; }

        [DbField("lastname")]
        public string LastName { get; set; }
    }
}