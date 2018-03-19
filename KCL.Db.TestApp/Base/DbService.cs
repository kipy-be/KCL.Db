using System;

namespace TestApp
{
    public class DbService : KCL.Db.Pgsql.DbService
    {
        public DbService(string host, ushort port, string dbName, string user, string password)
            : base(host, port, dbName, user, password)
        {
        }
    }
}
