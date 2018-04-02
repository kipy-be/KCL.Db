using KCL.Db.Pgsql;
using System;

namespace TestApp
{
    public class DbInterface : KCL.Db.Pgsql.DbInterface
    {
        public DbInterface(string host, ushort port, string dbName, string user, string password)
            : base(host, port, dbName, user, password)
        {
        }
    }
}
