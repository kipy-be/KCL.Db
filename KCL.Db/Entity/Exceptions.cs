using System;

namespace KCL.Db.Entity
{
    internal class EntityException : Exception
    {
        public EntityException()
        { }

        public EntityException(string message)
            : base(message)
        { }

        public EntityException(string message, params object[] obj)
            : base(String.Format(message, obj))
        { }

        public EntityException(string message, Exception inner)
            : base(message, inner)
        { }
    }

    internal class DbEntityConfigurationException : Exception
    {
        public DbEntityConfigurationException()
        { }

        public DbEntityConfigurationException(string message)
            : base(message)
        { }

        public DbEntityConfigurationException(string message, params object[] obj)
            : base(String.Format(message, obj))
        { }

        public DbEntityConfigurationException(string message, Exception inner)
            : base(message, inner)
        { }
    }

    internal class DbQueryException : Exception
    {
        public DbQueryException()
        { }

        public DbQueryException(string message)
            : base(message)
        { }

        public DbQueryException(string message, params object[] obj)
            : base(String.Format(message, obj))
        { }

        public DbQueryException(string message, Exception inner)
            : base(message, inner)
        { }
    }
}
