using System;
using System.Linq.Expressions;
using System.Text;

namespace KCL.Db.SqlBuilder
{
    public interface ISql
    {
        ISql From(string table, string alias = null);
        ISql From(ISql select, string alias);
        ISql Join(string left, string onLeft, string op, string onRight);
        ISql LeftJoin(string left, string onLeft, string op, string onRight);
        ISql RightJoin(string left, string onLeft, string op, string onRight);

        ISql Where(string left, string op, string right);
        ISql Where<T>(Expression<Func<T, bool>> expression);
        ISql OrderBy(string field, bool asc = true);

        string ToString();
    }

    public class Sql : ISql
    {
        private Sql()
        {}

        private StringBuilder _sql = new StringBuilder();
        private bool _whereSet = false;
        private bool _orderBySet = false;

        public new string ToString()
        {
            return _sql.ToString();
        }

        public void Clear()
        {
            _sql.Clear();
        }

        public static ISql Select(params string[] fields)
        {
            var sql = new Sql();
            sql._sql.AppendFormat("SELECT {0}", string.Join(", ", fields));
            return sql;
        }

        public static ISql Insert(string table)
        {
            var sql = new Sql();
            sql._sql.AppendFormat("INSERT INTO {0}", table);
            return sql;
        }

        public static ISql Update(string table)
        {
            var sql = new Sql();
            sql._sql.AppendFormat("UPDATE {0}", table);
            return sql;
        }

        public static ISql Delete(string table)
        {
            var sql = new Sql();
            sql._sql.AppendFormat("DELETE FROM {0}", table);
            return sql;
        }

        public ISql From(string table, string alias)
        {
            _sql.AppendFormat("\nFROM {0}", table);

            if (alias != null)
                _sql.AppendFormat(" AS {0}", alias);

            return this;
        }

        public ISql From(ISql select, string alias)
        {
            _sql.AppendFormat("\nFROM\n(\n    {0}\n) AS {1}", select.ToString().Replace("\n", "\n    "), alias);
            return this;
        }

        public ISql Join(string left, string onLeft, string op, string onRight)
        {
            _sql.AppendFormat("\nINNER JOIN {0} ON {1} {2} {3}", left, onLeft, op, onRight);
            return this;
        }

        public ISql LeftJoin(string left, string onLeft, string op, string onRight)
        {
            _sql.AppendFormat("\nOUTER LEFT JOIN {0} ON {1} {2} {3}", left, onLeft, op, onRight);
            return this;
        }

        public ISql RightJoin(string left, string onLeft, string op, string onRight)
        {
            _sql.AppendFormat("\nOUTER RIGHT JOIN {0} ON {1} {2} {3}", left, onLeft, op, onRight);
            return this;
        }

        public ISql Where(string left, string op, string right)
        {
            if (!_whereSet)
            {
                _sql.AppendFormat("\nWHERE ({0} {1} {2})", left, op, right);
                _whereSet = true;
            }
            else
                _sql.AppendFormat("\nAND ({0} {1} {2})", left, op, right);

            return this;
        }

        public ISql Where<T>(Expression<Func<T, bool>> expression)
        {
            string expBody = expression
                                .Body
                                .ToString()
                                .Replace("AndAlso", "AND")
                                .Replace("==", "=");

            if (!_whereSet)
            {
                _sql.AppendFormat("\nWHERE {0}", expBody);
                _whereSet = true;
            }
            else
                _sql.AppendFormat("\nAND {0}", expBody);

            return this;
        }

        public ISql OrderBy(string field, bool asc)
        {
            if (!_orderBySet)
            {
                _sql.AppendFormat("\nORDER BY {0} {1}", field, asc ? "ASC" : "DESC");
                _orderBySet = true;
            }
            else
                _sql.AppendFormat(", {0} {1}", field, asc ? "ASC" : "DESC");

            return this;
        }
    }
}
