namespace TeamTools.TSQL.Linter.Routines
{
    internal static class TSqlDomainAttributes
    {
        public const string TempTablePrefix = "#";
        public const char TempTablePrefixChar = '#';
        public const string VariablePrefix = "@";
        public const char VariablePrefixChar = '@';
        public const string NamePartSeparator = ".";
        public const string SystemSchemaName = "sys";
        public const string DefaultSchemaName = "dbo";
        public const string DefaultSchemaPrefix = "dbo.";
        public const string DefaultFileGroup = "PRIMARY";
        public const string DefaultFileGroupQuoted = "[PRIMARY]";
        public const string DateTimePartEnum = "DATE_TIME_PART"; // magic for linking info from metadata resource

        public static bool IsTriggerSystemTable(string tableName)
        {
            return tableName.Equals(TriggerSystemTables.Inserted, System.StringComparison.OrdinalIgnoreCase)
                || tableName.Equals(TriggerSystemTables.Deleted, System.StringComparison.OrdinalIgnoreCase);
        }

        public static class TriggerSystemTables
        {
            public const string Inserted = "INSERTED";
            public const string Deleted = "DELETED";
        }

        public static class Types
        {
            public const string Bit = "BIT";
            public const string SmallInt = "SMALLINT";
            public const string TinyInt = "TINYINT";
            public const string Int = "INT";
            public const string BigInt = "BIGINT";

            public const string Decimal = "DECIMAL";
            public const string Float = "FLOAT";

            public const string Char = "CHAR";
            public const string Varchar = "VARCHAR";
            public const string NChar = "NCHAR";
            public const string NVarchar = "NVARCHAR";
            public const string SysName = "SYSNAME";

            public const string Binary = "BINARY";
            public const string VarBinary = "VARBINARY";
        }
    }
}
