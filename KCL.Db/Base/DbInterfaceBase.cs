using System;
using System.Collections.Generic;
using System.Data;
using KCL.Db.Entity;

namespace KCL.Db
{
    public interface IDbInterface
    {
        bool IsConnected { get; }

        void Connect();
        IDbTransaction CreateTransaction();
        IDbTransaction CreateTransaction(IsolationLevel isolationLevel);
        void Delete<TEntity>(TEntity entity) where TEntity : DbEntity<TEntity>;
        void DeleteWhere<TEntity>(IEnumerable<KeyValuePair<string, object>> where) where TEntity : DbEntity<TEntity>;
        void Dispose();
        int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null);
        IDataReader ExecuteReader(string query, Dictionary<string, object> parameters = null);
        object ExecuteScalar(string query, Dictionary<string, object> parameters = null);
        Dictionary<string, bool> GetColumns(IDataReader reader);
        void Insert<TEntity>(TEntity entity) where TEntity : DbEntity<TEntity>;
        List<TEntity> ParseMany<TEntity>(string query, Dictionary<string, object> parameters = null) where TEntity : DbEntity<TEntity>, new();
        TEntity ParseOne<TEntity>(string query, Dictionary<string, object> parameters) where TEntity : DbEntity<TEntity>, new();
        void Update<TEntity>(TEntity entity) where TEntity : DbEntity<TEntity>;
        void UpdateWhere<TEntity>(IEnumerable<KeyValuePair<string, object>> set, IEnumerable<KeyValuePair<string, object>> where) where TEntity : DbEntity<TEntity>;
    }

    public abstract class DbInterfaceBase : IDisposable, IDbInterface
    {
        protected IDbConnection _connection;
        protected bool _isConnected = false;

        protected DbInterfaceBase(string connectionString)
        {
            _connection = CreateConnection(connectionString);
        }

        protected DbInterfaceBase(string host, ushort port, string dbName, string user, string password)
        {
            _connection = CreateConnection(host, port, dbName, user, password);
        }

        protected abstract IDbCommand CreateCommand(string query);
        protected abstract IDbDataParameter ToDataParameter(string id, object o);
        protected abstract IDbConnection CreateConnection(string connectionString);
        protected abstract IDbConnection CreateConnection(string host, ushort port, string dbName, string user, string password);
        protected abstract string ToInlinedValue(object o);
        protected abstract object GetIncrement(string table, string field, string sequence = null);

        protected virtual string GetTestQuery() => "SELECT 1";
        protected virtual string GetInsertquery() => "INSERT INTO {0} ({1}) VALUES({2})";
        protected virtual string GetUpdatequery() => "UPDATE {0} SET {1} WHERE {2}";
        protected virtual string GetDeletequery() => "DELETE FROM {0} WHERE {1}";

        public abstract IDbTransaction CreateTransaction();
        public abstract IDbTransaction CreateTransaction(IsolationLevel isolationLevel);

        public void Connect()
        {
            try
            {
                _connection.Open();
                _isConnected = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool IsConnected
        {
            get
            {
                if (!_isConnected)
                    return false;
                else if (!(_connection.State == ConnectionState.Open))
                    return false;
                else
                {
                    try
                    {
                        ExecuteNonQuery(GetTestQuery());
                        
                        if (!_isConnected)
                            _isConnected = true;
                    }
                    catch (Exception)
                    {
                        if (_isConnected)
                            _isConnected = false;
                    }

                    return _isConnected;
                }
            }
        }

        protected IDbCommand CreateCommand(string query, Dictionary<string, object> parameters = null)
        {
            IDbCommand command = null;

            try
            {
                command = CreateCommand(query);

                if (parameters != null)
                {
                    foreach (var kvp in parameters)
                        command.Parameters.Add(ToDataParameter(kvp.Key, kvp.Value));
                }
            }
            catch(Exception)
            {
                throw new DataException("Error while binding parameters : unable to bind parameters");
            }

            return command;
        }

        public IDataReader ExecuteReader(string query, Dictionary<string, object> parameters = null)
        {
            using (IDbCommand cmd = CreateCommand(query, parameters))
            {
                return cmd.ExecuteReader();
            }
        }

        public int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            using (IDbCommand cmd = CreateCommand(query, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            using (IDbCommand cmd = CreateCommand(query, parameters))
            {
                return cmd.ExecuteScalar();
            }
        }

        public Dictionary<string, bool> GetColumns(IDataReader reader)
        {
            Dictionary<string, bool> columns = new Dictionary<string, bool>();
            for (int i = 0; i < reader.FieldCount; i++)
                columns.Add(reader.GetName(i), true);
            return columns;
        }

        protected string MakeWhereClause(IEnumerable<KeyValuePair<string, object>> values)
        {
            bool first = true;
            string where = "";
            foreach (var kvp in values)
            {
                if (!first)
                    where += " AND ";
                else
                    first = false;

                where += kvp.Key + " = " + ToInlinedValue(kvp.Value);
            }

            return where;
        }

        protected string MakeSetClause(IEnumerable<KeyValuePair<string, object>> values)
        {
            bool first = true;
            string where = "";
            foreach (var kvp in values)
            {
                if (!first)
                    where += ", ";
                else
                    first = false;

                where += kvp.Key + " = " + ToInlinedValue(kvp.Value);
            }

            return where;
        }

        protected string GetInsertQuery(string table, IEnumerable<KeyValuePair<string, object>> values)
        {
            bool first = true;
            string cn = "";
            string vn = "";
            foreach (var kvp in values)
            {
                if (kvp.Value != null)
                {
                    if (!first)
                    {
                        vn += ", ";
                        cn += ", ";
                    }
                    else
                        first = false;

                    cn += kvp.Key;
                    vn += ToInlinedValue(kvp.Value);
                }
            }

            return string.Format(GetInsertquery(), table, cn, vn);
        }

        protected string GetUpdateQuery(string table, IEnumerable<KeyValuePair<string, object>> values, IEnumerable<KeyValuePair<string, object>> whereValues)
        {
            bool first = true;
            string cvn = "";
            foreach (var kvp in values)
            {
                if (!first)
                    cvn += ", ";
                else
                    first = false;

                cvn += kvp.Key + " = " + ToInlinedValue(kvp.Value);
            }

            return string.Format(GetUpdatequery(), table, cvn, MakeWhereClause(whereValues));
        }

        protected string GetDeleteQuery(string table, IEnumerable<KeyValuePair<string, object>> whereValues)
        {
            return string.Format(GetDeletequery(), table, MakeWhereClause(whereValues));
        }

        public TEntity ParseOne<TEntity>(string query, Dictionary<string, object> parameters)
            where TEntity : DbEntity<TEntity>, new()
        {
            IDataReader reader = null;

            try
            {
                reader = ExecuteReader(query, parameters);
                if (reader.Read())
                    return DbEntity<TEntity>.Parse(reader, GetColumns(reader));
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.CloseAndDispose();
            }
        }

        public List<TEntity> ParseMany<TEntity>(string query, Dictionary<string, object> parameters = null)
            where TEntity : DbEntity<TEntity>, new()
        {
            IDataReader reader = null;
            var res = new List<TEntity>();

            try
            {
                reader = ExecuteReader(query, parameters);
                if (reader.Read())
                {
                    var columns = GetColumns(reader);
                    do
                        res.Add(DbEntity<TEntity>.Parse(reader, columns));
                    while (reader.Read());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.CloseAndDispose();
            }

            return res;
        }

        public virtual void Insert<TEntity>(TEntity entity)
            where TEntity : DbEntity<TEntity>
        { 
            if (!DbEntity<TEntity>.EntityInfo.HasSeededKey)
            {
                ExecuteNonQuery(GetInsertQuery
                (
                    DbEntity<TEntity>.EntityInfo.Table,
                    entity.GetValues(true, true, true)
                ));
            }
            else
            {
                ExecuteNonQuery(GetInsertQuery
                (
                    DbEntity<TEntity>.EntityInfo.Table,
                    entity.GetValues(false, true, true)
                ));

                var value = Convert.ChangeType
                (
                    GetIncrement
                    (
                        DbEntity<TEntity>.EntityInfo.Table,
                        DbEntity<TEntity>.EntityInfo.SeededKey.Name,
                        DbEntity<TEntity>.EntityInfo.SeededKey.Sequence
                    ),
                    DbEntity<TEntity>.EntityInfo.SeededKey.Type
                );
                entity.SetValue(value, DbEntity<TEntity>.EntityInfo.SeededKey.PropertyName);
            }
        }

        public virtual void Update<TEntity>(TEntity entity)
            where TEntity : DbEntity<TEntity>
        {
            ExecuteNonQuery(GetUpdateQuery
            (
                DbEntity<TEntity>.EntityInfo.Table,
                entity.GetValues(false, true, true),
                entity.GetValues(true, false, false)
            ));
        }

        public virtual void UpdateWhere<TEntity>(IEnumerable<KeyValuePair<string, object>> set, IEnumerable<KeyValuePair<string, object>> where)
            where TEntity : DbEntity<TEntity>
        {
            ExecuteNonQuery(string.Format
            (
                GetUpdatequery(),
                DbEntity<TEntity>.EntityInfo.Table,
                MakeSetClause(set),
                MakeWhereClause(where)
            ));
        }

        public virtual void Delete<TEntity>(TEntity entity)
            where TEntity : DbEntity<TEntity>
        {
            ExecuteNonQuery(GetDeleteQuery
            (
                DbEntity<TEntity>.EntityInfo.Table,
                entity.GetValues(true, false, false)
            ));
        }

        public void DeleteWhere<TEntity>(IEnumerable<KeyValuePair<string, object>> where)
            where TEntity : DbEntity<TEntity>
        {
            ExecuteNonQuery(string.Format
            (
                GetDeletequery(),
                DbEntity<TEntity>.EntityInfo.Table,
                MakeWhereClause(where)
            ));
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }
    }
}
