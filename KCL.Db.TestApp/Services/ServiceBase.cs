using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp.Services
{
    public abstract class ServiceBase
    {
        protected DbService _db;

        public ServiceBase(DbService db)
        {
            _db = db;
        }
    }
}
