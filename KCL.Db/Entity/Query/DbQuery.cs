using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace KCL.Db.Entity.Query
{
    public class DbQuery<TEntity>
        where TEntity : DbEntity<TEntity>, new()
    {
        private IDbInterface _dbInterface;
        private bool _whereSet = false;
        private bool _orderBySet = false;
        private StringBuilder _sql = new StringBuilder();

        public DbQuery(IDbInterface dbInterface)
        {
            _dbInterface = dbInterface;
            _sql = new StringBuilder();

            _sql.AppendFormat
            (
                "SELECT {0} " +
                "\nFROM {1} ",
                string.Join(", ", DbEntity<TEntity>.GetFields(true, true, true)),
                DbEntity<TEntity>.EntityInfo.Table
            );
        }

        public new string ToString()
        {
            return _sql.ToString();
        }

        public void Clear()
        {
            _sql.Clear();
        }

        public TEntity GetOne()
        {
            return _dbInterface.ParseOne<TEntity>(ToString(), null);
        }

        public DbQuery<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            var translator = new QueryTranslator<TEntity>();
            var whereExp = translator.Translate(predicate);

            if (!_whereSet)
            {
                _sql.AppendFormat("\nWHERE {0}", whereExp);
                _whereSet = true;
            }
            else
                _sql.AppendFormat("\nAND {0}", whereExp);


            return this;
        }
    }
}
