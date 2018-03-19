using System;

namespace KCL.Db.Base
{
    public interface IDbService
    {
        IDbInterface DbInterface { get; }
    }

    public abstract class DbServiceBase : IDbService
    {
        public IDbInterface DbInterface { get; protected set; }
    }
}
