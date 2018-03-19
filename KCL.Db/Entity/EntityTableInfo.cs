using System;
using System.Collections;
using System.Collections.Generic;

namespace KCL.Db.Entity
{
    internal class EntityField
    {
        public string PropertyName { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    internal class EntityKey : EntityField
    {
        public bool IsAutoIncremented { get; set; }
        public string Sequence { get; set; }
    }

    internal class EntityRelation : EntityField
    {
        public string RelatedTable { get; set; }
        public string TableKey { get; set; }
        public string RelatedTableKey { get; set; }
        public string Prefix { get; set; }
    }

    internal class EntityInfo
    {
        public string Table { get; set; }

        public Dictionary<string, EntityKey> PrimaryKeys { get; set; }
        public Dictionary<string, EntityField> Fields { get; set; }
        public Dictionary<string, EntityRelation> RelationsToOne { get; set; }
        public Dictionary<string, EntityRelation> RelationsToMany { get; set; }
        public Dictionary<string, Action<DbEntityBase, object>> Setters { get; set; }
        public Dictionary<string, Func<DbEntityBase, object>> Getters { get; set; }
        public EntityKey SeededKey { get; set; }
        public Func<DbEntityBase> Factory { get; set; }
        public Func<IList> ListFactory { get; set; }
        public bool IsCompositeKey => PrimaryKeys.Count > 0;
        public bool HasSeededKey => SeededKey != null;

        public EntityInfo()
        {
            PrimaryKeys = new Dictionary<string, EntityKey>();
            Fields = new Dictionary<string, EntityField>();
            RelationsToOne = new Dictionary<string, EntityRelation>();
            RelationsToMany = new Dictionary<string, EntityRelation>();
            Setters = new Dictionary<string, Action<DbEntityBase, object>>();
            Getters = new Dictionary<string, Func<DbEntityBase, object>>();
        }
    }
}
