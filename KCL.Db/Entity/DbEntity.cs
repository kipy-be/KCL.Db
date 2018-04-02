using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace KCL.Db.Entity
{
    public abstract class DbEntityBase
    {
        internal static ConcurrentDictionary<string, EntityInfo> EntityTablesInfos = new ConcurrentDictionary<string, EntityInfo>();

        public abstract T GetValue<T>([CallerMemberName] string name = null);
        public abstract void SetValue<T>(T value, [CallerMemberName] string name = null);
    }

    public abstract class DbEntity<TEntity> : DbEntityBase
        where TEntity : DbEntityBase
    {
        private static bool _isRegistered = false;
        private static bool _isRegistering = false;
        internal static EntityInfo EntityInfo;

        static DbEntity()
        {
            Register();
        }

        internal static void Register()
        {
            if (!_isRegistered && !_isRegistering)
            {
                _isRegistering = true;

                EntityInfo = new EntityInfo
                {
                    Factory = CreateFactory<TEntity>(),
                    ListFactory = CreateFactory<List<TEntity>>()
                };

                var entityType = typeof(TEntity);
                var entityTypeInfo = entityType.GetTypeInfo();

                var attributes = entityTypeInfo.GetCustomAttributes(false);
                bool tableSet = false;
                foreach (Attribute attr in attributes)
                {
                    if (attr is DbTable)
                    {
                        DbTable dbTable = attr as DbTable;
                        EntityInfo.Table = dbTable.Name;

                        tableSet = true;
                    }
                }

                if (!tableSet)
                    throw new EntityException("Error while registering entity {0} : no DbTable attribute found to match table", entityType.Name);

                PropertyInfo[] properties = entityType.GetProperties();

                int nbKeys = 0;
                foreach (PropertyInfo property in properties)
                {
                    attributes = property.GetCustomAttributes(false);

                    foreach (var attribute in attributes)
                    {
                        if (attribute is DbKey)
                        {
                            var dbKey = attribute as DbKey;

                            if (EntityInfo.PrimaryKeysByFieldName.ContainsKey(dbKey.Name))
                                throw new EntityException("Error while registering entity {0} : duplicate primary key detected ({1})", entityType.Name, dbKey.Name);

                            var key = new EntityKey() { IsAutoIncremented = dbKey.IsAutoIncremented, Sequence = dbKey.Sequence, Name = dbKey.Name, PropertyName = property.Name, Type = property.PropertyType };

                            EntityInfo.AddEntityKey(key);

                            EntityInfo.Setters.Add(property.Name, CreateSetter(property.Name, property.PropertyType));
                            EntityInfo.Getters.Add(property.Name, CreateGetter(property.Name));
                            nbKeys++;

                            if (dbKey.IsAutoIncremented)
                            {
                                if (EntityInfo.HasSeededKey)
                                    throw new EntityException("Error while registering entity {0} : only one seeded column is authorized", entityType.Name);

                                EntityInfo.SeededKey = key;
                            }
                        }
                        else if (attribute is DbChildRelation)
                        {
                            var dbRelation = attribute as DbChildRelation;
                            var relation = new EntityRelation() { RelatedTable = dbRelation.ChildTable, TableKey = dbRelation.TableKey, RelatedTableKey = dbRelation.ChildTableKey, Prefix = dbRelation.Prefix, Type = property.PropertyType, PropertyName = property.Name };

                            RegisterChildIfNeeded(property.PropertyType);

                            if (property.PropertyType.IsGenericParameter)
                                EntityInfo.AddEntityRelationsToMany(relation);
                            else
                            {
                                EntityInfo.AddEntityRelationsToOne(relation);

                                EntityInfo.Setters.Add(property.Name, CreateSetter(property.Name, property.PropertyType));
                                EntityInfo.Getters.Add(property.Name, CreateGetter(property.Name));
                            }
                        }
                        else if (attribute is DbParentRelation)
                        {
                            var dbRelation = attribute as DbParentRelation;
                            var relation = new EntityRelation() { Name = dbRelation.TableKey, RelatedTable = dbRelation.ParentTable, TableKey = dbRelation.TableKey, RelatedTableKey = dbRelation.ParentTableKey, Prefix = dbRelation.Prefix, Type = property.PropertyType, PropertyName = property.Name };

                            RegisterChildIfNeeded(property.PropertyType);

                            if (property.PropertyType.IsGenericParameter)
                                EntityInfo.AddEntityRelationsToMany(relation);
                            else
                            {
                                EntityInfo.AddEntityRelationsToOne(relation);

                                EntityInfo.Setters.Add(property.Name, CreateSetter(property.Name, property.PropertyType));
                                EntityInfo.Getters.Add(property.Name, CreateGetter(property.Name));
                            }
                        }
                        else if (attribute is DbField)
                        {
                            var dbField = attribute as DbField;

                            if (EntityInfo.FieldsByFieldName.ContainsKey(dbField.Name))
                                throw new EntityException("Error while registering entity {0} : duplicate field name detected ({1})", entityType.Name, dbField.Name);

                            var field = new EntityField() { Name = dbField.Name, Type = property.PropertyType, PropertyName = property.Name };

                            EntityInfo.AddEntityField(field);

                            EntityInfo.Setters.Add(property.Name, CreateSetter(property.Name, property.PropertyType));
                            EntityInfo.Getters.Add(property.Name, CreateGetter(property.Name));
                        }
                    }
                }

                EntityTablesInfos.TryAdd(EntityInfo.Table, EntityInfo);

                _isRegistered = true;
                _isRegistering = false;
            }
        }

        private static void RegisterChildIfNeeded(Type type)
        {
            var register = type.GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            register?.Invoke(null, null);
        }

        private static Func<T> CreateFactory<T>()
        {
            var createNew = Expression.New(typeof(T));
            var lambda = Expression.Lambda<Func<T>>(createNew);

            return lambda.Compile();
        }

        private static Action<DbEntityBase, object> CreateSetter(string propertyName, Type propertyType)
        {
            var param1 = Expression.Parameter(typeof(DbEntityBase), "o");
            var param2 = Expression.Parameter(typeof(object), "v");
            var property = Expression.Property(Expression.Convert(param1, typeof(TEntity)), propertyName);
            var expression = Expression.Assign(property, Expression.Convert(param2, propertyType));

            var lambda = Expression.Lambda<Action<DbEntityBase, object>>(expression, param1, param2);

            return lambda.Compile();
        }

        private static Func<DbEntityBase, object> CreateGetter(string propertyName)
        {
            var param = Expression.Parameter(typeof(DbEntityBase), "o");
            var expression = Expression.Convert(Expression.Property(Expression.Convert(param, typeof(TEntity)), propertyName), typeof(object));

            var lambda = Expression.Lambda<Func<DbEntityBase, object>>(expression, param);

            return lambda.Compile();
        }

        public override T GetValue<T>([CallerMemberName] string name = null)
        {
            var v = EntityInfo.Getters[name](this);
            return v != null ? (T)v : default(T);
        }

        public override void SetValue<T>(T value, [CallerMemberName] string name = null)
        {
            EntityInfo.Setters[name](this, value);
        }

        public static TEntity Parse(IDataReader reader, Dictionary<string, bool> columns)
        {
            return (TEntity)Parse(EntityInfo.Table, reader, columns);
        }

        private static DbEntityBase Parse(string table, IDataReader reader, Dictionary<string, bool> columns, string prefix = null)
        {
            if (!EntityTablesInfos.ContainsKey(table))
                throw new EntityException("table {0} is not registered", table);

            EntityInfo tableInfo = EntityTablesInfos[table];
            var entity = tableInfo.Factory();

            foreach (var field in tableInfo.PrimaryKeysByFieldName)
            {
                var key = prefix != null ? prefix + "_" + field.Key : field.Key;

                if (columns.ContainsKey(key) && reader[key] != DBNull.Value && tableInfo.Setters.ContainsKey(field.Value.PropertyName))
                {
                    tableInfo.Setters[field.Value.PropertyName](entity, reader[key]);
                }
            }

            foreach (var field in tableInfo.FieldsByFieldName)
            {
                var key = prefix != null ? prefix + "_" + field.Key : field.Key;

                if (columns.ContainsKey(key) && reader[key] != DBNull.Value && tableInfo.Setters.ContainsKey(field.Value.PropertyName))
                {
                    tableInfo.Setters[field.Value.PropertyName](entity, reader[key]);
                }
            }

            foreach (var relation in tableInfo.RelationsToOneByFieldName)
            {
                if (columns.ContainsKey(relation.Key) && reader[relation.Key] != DBNull.Value && tableInfo.Setters.ContainsKey(relation.Value.PropertyName))
                {
                    var entityRelation = tableInfo.RelationsToOneByFieldName[relation.Key];
                    tableInfo.Setters[relation.Value.PropertyName](entity, Parse(entityRelation.RelatedTable, reader, columns, entityRelation.Prefix));
                }
            }

            return entity;
        }

        internal static IEnumerable<string> GetFields(bool keys, bool fields, bool relations)
        {
            if (keys)
                foreach (var kvp in EntityInfo.PrimaryKeysByFieldName)
                    yield return kvp.Value.Name;

            if (fields)
                foreach (var kvp in EntityInfo.FieldsByFieldName)
                    yield return kvp.Value.Name;

            if (relations)
                foreach (var kvp in EntityInfo.RelationsToOneByFieldName)
                    yield return kvp.Value.TableKey;
        }

        internal IEnumerable<KeyValuePair<string, object>> GetValues(bool keys, bool fields, bool relations)
        {
            if(keys)
                foreach (var kvp in EntityInfo.PrimaryKeysByFieldName)
                    yield return new KeyValuePair<string, object>(kvp.Value.Name, GetValue<object>(kvp.Value.PropertyName));

            if(fields)
                foreach (var kvp in EntityInfo.FieldsByFieldName)
                    yield return new KeyValuePair<string, object>(kvp.Value.Name, GetValue<object>(kvp.Value.PropertyName));

            if(relations)
                foreach (var kvp in EntityInfo.RelationsToOneByFieldName)
                    yield return new KeyValuePair<string, object>(kvp.Value.TableKey, GetValue<object>(kvp.Value.PropertyName));
        }
    }
}
