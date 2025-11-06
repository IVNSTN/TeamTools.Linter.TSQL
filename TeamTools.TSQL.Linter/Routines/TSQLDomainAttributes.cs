namespace TeamTools.TSQL.Linter.Routines
{
    internal static class TSqlDomainAttributes
    {
        public const string TempTablePrefix = "#";
        public const char TempTablePrefixChar = '#';
        public const string VariablePrefix = "@";
        public const char VariablePrefixChar = '@';
        public const string NamePartSeparator = ".";
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
    }
}
