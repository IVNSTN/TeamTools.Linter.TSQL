namespace TeamTools.TSQL.ExpressionEvaluator.Routines
{
    // TODO : this is a copy of TSqlDomainAttributes from TSQL linter library - get rid of duplication
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

            public const string SmallDateTime = "SMALLDATETIME";
            public const string DateTime = "DATETIME";
            public const string DateTime2 = "DATETIME2";
            public const string Date = "DATE";
            public const string Time = "TIME";
        }
    }
}
