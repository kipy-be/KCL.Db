using System;
using System.Collections;
using System.Collections.Generic;

namespace KCL.Db.Entity
{
    internal class EntityBaseField
    {
        public string PropertyName { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    internal class EntityField : EntityBaseField
    {
    }

    internal class EntityKey : EntityBaseField
    {
        public bool IsAutoIncremented { get; set; }
        public string Sequence { get; set; }
    }

    internal class EntityRelation : EntityBaseField
    {
        public string RelatedTable { get; set; }
        public string TableKey { get; set; }
        public string RelatedTableKey { get; set; }
        public string Prefix { get; set; }
    }

    internal class EntityInfo
    {
        public string Table { get; set; }

        public Dictionary<string, EntityBaseField> BaseFieldsByFieldName { get; private set; }
        public Dictionary<string, EntityBaseField> BaseFieldsByPropertyName { get; private set; }

        public Dictionary<string, EntityKey> PrimaryKeysByFieldName { get; private set; }
        public Dictionary<string, EntityField> FieldsByFieldName { get; private set; }
        public Dictionary<string, EntityRelation> RelationsToOneByFieldName { get; private set; }
        public Dictionary<string, EntityRelation> RelationsToManyByFieldName { get; private set; }

        public Dictionary<string, EntityKey> PrimaryKeysByPropertyName { get; private set; }
        public Dictionary<string, EntityField> FieldsByPropertyName { get; private set; }
        public Dictionary<string, EntityRelation> RelationsToOneByPropertyName { get; private set; }
        public Dictionary<string, EntityRelation> RelationsToManyByPropertyName { get; private set; }

        public Dictionary<string, Action<DbEntityBase, object>> Setters { get; private set; }
        public Dictionary<string, Func<DbEntityBase, object>> Getters { get; private set; }

        public EntityKey SeededKey { get; set; }
        public Func<DbEntityBase> Factory { get; set; }
        public Func<IList> ListFactory { get; set; }
        public bool IsCompositeKey => PrimaryKeysByFieldName.Count > 0;
        public bool HasSeededKey => SeededKey != null;

        public EntityInfo()
        {
            BaseFieldsByFieldName = new Dictionary<string, EntityBaseField>();
            BaseFieldsByPropertyName = new Dictionary<string, EntityBaseField>();

            PrimaryKeysByFieldName = new Dictionary<string, EntityKey>();
            FieldsByFieldName = new Dictionary<string, EntityField>();
            RelationsToOneByFieldName = new Dictionary<string, EntityRelation>();
            RelationsToManyByFieldName = new Dictionary<string, EntityRelation>();

            PrimaryKeysByPropertyName = new Dictionary<string, EntityKey>();
            FieldsByPropertyName = new Dictionary<string, EntityField>();
            RelationsToOneByPropertyName = new Dictionary<string, EntityRelation>();
            RelationsToManyByPropertyName = new Dictionary<string, EntityRelation>();

            Setters = new Dictionary<string, Action<DbEntityBase, object>>();
            Getters = new Dictionary<string, Func<DbEntityBase, object>>();
        }

        internal void AddEntityKey(EntityKey key)
        {
            BaseFieldsByFieldName.Add(key.Name, key);
            BaseFieldsByPropertyName.Add(key.PropertyName, key);
            PrimaryKeysByFieldName.Add(key.Name, key);
            PrimaryKeysByPropertyName.Add(key.PropertyName, key);
        }

        internal void AddEntityField(EntityField field)
        {
            BaseFieldsByFieldName.Add(field.Name, field);
            BaseFieldsByPropertyName.Add(field.PropertyName, field);
            FieldsByFieldName.Add(field.Name, field);
            FieldsByPropertyName.Add(field.PropertyName, field);
        }

        internal void AddEntityRelationsToOne(EntityRelation relation)
        {
            BaseFieldsByFieldName.Add(relation.TableKey, relation);
            BaseFieldsByPropertyName.Add(relation.PropertyName, relation);
            RelationsToOneByFieldName.Add(relation.TableKey, relation);
            RelationsToOneByPropertyName.Add(relation.PropertyName, relation);
        }

        internal void AddEntityRelationsToMany(EntityRelation relation)
        {
            BaseFieldsByFieldName.Add(relation.TableKey, relation);
            BaseFieldsByPropertyName.Add(relation.PropertyName, relation);
            RelationsToManyByFieldName.Add(relation.TableKey, relation);
            RelationsToManyByPropertyName.Add(relation.PropertyName, relation);
        }
    }
}
