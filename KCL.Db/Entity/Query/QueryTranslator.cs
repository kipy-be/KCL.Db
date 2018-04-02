using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace KCL.Db.Entity.Query
{
    public abstract class QueryTranslator
    {
        protected static ConcurrentDictionary<string, Func<object, object>> _getters;

        static QueryTranslator()
        {
            _getters = new ConcurrentDictionary<string, Func<object, object>>();
        }
    }

    public class QueryTranslator<TEntity> : QueryTranslator
        where TEntity : DbEntity<TEntity>
    {
        private StringBuilder _res;
        private string _paramName;

        public string Translate(Expression<Func<TEntity, bool>> predicate)
        {
            _res = new StringBuilder();
            _paramName = predicate.Parameters[0].Name;
            Parse(predicate);

            return _res.ToString();
        }

        private void Parse(Expression<Func<TEntity, bool>> expression)
        {
            var param = expression.Parameters[0];
            var operation = (BinaryExpression)expression.Body;

            ParseExpression(operation);
        }

        private void ParseExpression(Expression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    ParseMemberAccess((MemberExpression)exp);
                    break;

                case ExpressionType.Constant:
                    ParseConstant((ConstantExpression)exp);
                    break;

                case ExpressionType.Equal:
                    ParseEval((BinaryExpression)exp, "=");
                    break;

                case ExpressionType.NotEqual:
                    ParseEval((BinaryExpression)exp, "<>");
                    break;

                case ExpressionType.GreaterThan:
                    ParseEval((BinaryExpression)exp, ">");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    ParseEval((BinaryExpression)exp, ">=");
                    break;

                case ExpressionType.LessThan:
                    ParseEval((BinaryExpression)exp, "<");
                    break;

                case ExpressionType.LessThanOrEqual:
                    ParseEval((BinaryExpression)exp, ">=");
                    break;

                case ExpressionType.AndAlso:
                    ParseOperand((BinaryExpression)exp, "AND");
                    break;

                case ExpressionType.OrElse:
                    ParseOperand((BinaryExpression)exp, "OR");
                    break;

                default:
                    throw new DbQueryException("Error while translating query : unsupported operation {0}", exp.NodeType.ToString());
            }
        }

        private void ParseMemberAccess(MemberExpression exp)
        {
            if (exp.Expression.ToString() == _paramName)
                _res.Append(DbEntity<TEntity>.EntityInfo.BaseFieldsByPropertyName[exp.Member.Name].Name);
            else
            {
                string expId = exp.ToString();
                Func<object, object> getter;

                if (!_getters.ContainsKey(expId))
                {
                    getter = CreateGetter(exp);
                    _getters.TryAdd(expId, getter);
                }
                else
                    getter = _getters[expId];

                var constant = GetConstantExpression(exp).Value;
                _res.AppendFormat(ToInlinedValue(getter(constant)));
            }
        }

        private ConstantExpression GetConstantExpression(MemberExpression exp)
        {
            if (exp.Expression.NodeType == ExpressionType.Constant)
                return (ConstantExpression)exp.Expression;
            else
                return GetConstantExpression((MemberExpression)exp.Expression);
        }

        private Func<object, object> CreateGetter(MemberExpression exp)
        {
            var param = Expression.Parameter(typeof(object), "o");
            var getter = Expression.Convert(CreateGetterLambda(param, exp, exp), typeof(object));

            var lambda = Expression.Lambda<Func<object, object>>(getter, param);

            return lambda.Compile();
        }

        private Expression CreateGetterLambda(ParameterExpression param, Expression getter, Expression exp)
        {
            var me = (MemberExpression)exp;
            if (me.Expression.NodeType == ExpressionType.MemberAccess)
                getter = Expression.Property(CreateGetterLambda(param, getter, me.Expression), me.Member.Name);
            else
                getter = me.Update(Expression.Convert(param, me.Expression.Type));
            
            return getter;
        }

        private void ParseConstant(ConstantExpression exp)
        {
            _res.AppendFormat("{0}", ToInlinedValue(exp.Value));
        }

        private void ParseEval(BinaryExpression exp, string op)
        {
            ParseExpression(exp.Left);
            _res.Append(" " + op + " ");
            ParseExpression(exp.Right);
        }

        private void ParseOperand(BinaryExpression exp, string op)
        {
            _res.Append("(");
            ParseExpression(exp.Left);
            _res.Append(") " + op + " (");
            ParseExpression(exp.Right);
            _res.Append(")");
        }

        private string ToInlinedValue(object o)
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
