using System;
using System.Collections.Generic;
using TestApp;
using TestApp.Models;
using TestApp.Services;

namespace KCL.Db.TestApp
{
    public class Program
    {
        private static DbService _db;

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += ProcessExit;

            try
            {
                ApplicationContext.Init();
                _db = ApplicationContext.DbService;

                Test();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : {0}", ex.Message);
            }
            finally
            {
                Exit();
            }
        }

        private static void Exit()
        {
#if DEBUG
            Console.ReadLine();
#endif
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            Exit();
        }

        private static void Test()
        {
            var authorsService = new AuthorsService(_db);
            var articlesService = new ArticlesService(_db);

            var authors = authorsService.GetAll();
            var article = articlesService.GetFromId(1);
            article.Title = "test";

            var author2 = new Author()
            {
                Nick = "truc",
                FirstName = "Jean",
                LastName = "Bon"
            };

            Console.WriteLine(author2.Id);

            _db.DbInterface.Insert(author2);

            author2.LastName = "Bla";
            _db.DbInterface.Update(author2);

            //_db.Delete(author2);

            //_db.UpdateWhere<Author>(new Dictionary<string, object>
            //{
            //    { "firstname", "Thierry" }
            //},
            //new Dictionary<string, object>
            //{
            //    { "nick", "truc" }
            //});

            _db.DbInterface.DeleteWhere<Author>(new Dictionary<string, object>
            {
                { "nick", "truc" }
            });

            var transaction = _db.DbInterface.CreateTransaction();
            var author3 = new Author()
            {
                Nick = "machin",
                FirstName = "Pierre",
                LastName = "Test"
            };

            _db.DbInterface.Insert(author3);
            transaction.Commit();
        }
    }
}
