using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace KCL.Db.Pgsql
{
    public class DbInterface : DbInterfaceBase
    {
        public DbInterface(string connectionString)
            : base(connectionString)
        { }

        public DbInterface(string host, ushort port, string dbName, string user, string password)
            : base(host, port, dbName, user, password)
        { }

        protected override IDbCommand CreateCommand(string query)
        {
            return new NpgsqlCommand(query, (NpgsqlConnection)_connection);
        }

        protected override IDbConnection CreateConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        protected override IDbConnection CreateConnection(string host, ushort port, string dbName, string user, string password)
        {
            return new NpgsqlConnection(String.Format
            (
                "Host={0}; Port={1}; Username={2}; Password={3}; Database={4}",
                host, port, user, password, dbName
            ));
        }

        public override IDbTransaction CreateTransaction()
        {
            return _connection.BeginTransaction();
        }

        public override IDbTransaction CreateTransaction(IsolationLevel isolationLevel)
        {
            return _connection.BeginTransaction(isolationLevel);
        }

        protected override object GetIncrement(string table, string field, string sequence = null)
        {
            if(sequence != null)
                return ExecuteScalar($"SELECT CURRVAL('{sequence}')");
            else
                return ExecuteScalar($"SELECT currval(pg_get_serial_sequence('{table}', '{field}'))");
        }

        protected override IDbDataParameter ToDataParameter(string label, object o)
        {
            NpgsqlParameter param = null;

            if (o is string)
                param = new NpgsqlParameter(label, NpgsqlDbType.Varchar);
            else if (o is int || o is uint)
                param = new NpgsqlParameter(label, NpgsqlDbType.Integer);
            else if (o is long)
                param = new NpgsqlParameter(label, NpgsqlDbType.Bigint);
            else if (o is float || o is double)
                param = new NpgsqlParameter(label, NpgsqlDbType.Double);
            else if (o is DateTime)
                param = new NpgsqlParameter(label, NpgsqlDbType.Timestamp);
            else if (o is bool)
                param = new NpgsqlParameter(label, NpgsqlDbType.Boolean);

            param.NpgsqlValue = o;

            return param;
        }

        protected override string ToInlinedValue(object o)
        {
            if (o == null)
                return "NULL";

            if (o is string)
                return "'" + ((string)o).Replace("'", "''") + "'";
            else if (o is int || o is uint || o is long)
                return o.ToString();
            else if (o is float || o is double || o is decimal)
                return o.ToString().Replace(",", ".");
            else if (o is DateTime)
                return ((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss");
            else if (o is bool)
                return ((bool)o) ? "true" : "false";
            else
                return "NULL";
        }
    }
}
