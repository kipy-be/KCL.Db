using System;

namespace KCL.Db.Entity
{
    public class DbEntityAttribute : Attribute
    {}

    [AttributeUsage(AttributeTargets.Class)]
    public class DbTable : DbEntityAttribute
    {
        public string Name { get; private set; }

        public DbTable(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DbField : DbEntityAttribute
    {
        public string Name { get; private set; }

        public DbField(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DbKey : DbField
    {
        public bool   IsAutoIncremented { get; private set; }
        public string Sequence          { get; private set; }

        public DbKey(string name, bool isAutoIncremented = true, string sequence = null)
            : base(name)
        {
            IsAutoIncremented = isAutoIncremented;
            Sequence = sequence;
        }
    }

    public enum DbRelationType
    {
        One,
        Many
    }

    public class DbRelation : DbEntityAttribute
    {
        public string TableKey { get; protected set; }
        public string Prefix   { get; protected set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DbChildRelation : DbRelation
    {
        public string ChildTable    { get; private set; }
        public string ChildTableKey { get; private set; }
        

        public DbChildRelation(string childTable, string tableKey, string childTableKey = null, string prefix = null)
        {
            ChildTable    = childTable;
            TableKey      = tableKey;
            ChildTableKey = childTableKey != null ? childTableKey : tableKey;
            Prefix        = prefix;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DbParentRelation : DbRelation
    {
        public string ParentTable    { get; private set; }
        public string ParentTableKey { get; private set; }

        public DbParentRelation(string parentTable, string tableKey, string parentTableKey = null, string prefix = null)
        {
            ParentTable = parentTable;
            TableKey = tableKey;
            ParentTableKey = parentTableKey != null ? parentTableKey : tableKey;
            Prefix = prefix;
        }
    }
}
