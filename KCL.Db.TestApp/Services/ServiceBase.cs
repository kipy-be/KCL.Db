using System;

namespace TestApp.Services
{
    public abstract class ServiceBase
    {
        protected DbInterface _db;

        public ServiceBase(DbInterface db)
        {
            _db = db;
        }
    }
}
