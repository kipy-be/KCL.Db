using System;
using KCL.Db.Base;

namespace KCL.Db.Pgsql
{
    public abstract class DbService : DbServiceBase
    {
        public DbService(string connectionString)
        {
            DbInterface = new DbInterface(connectionString);
        }

        public DbService(string host, ushort port, string dbName, string user, string password)
        {
            DbInterface = new DbInterface(host, port, dbName, user, password);
        }
    }
}
