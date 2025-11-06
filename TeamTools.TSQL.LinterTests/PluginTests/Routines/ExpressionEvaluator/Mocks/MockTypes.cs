using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public static class MockTypes
    {
        private static readonly ICollection<string> Types = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "dbo.VARCHAR",
            "dbo.NVARCHAR",
            "dbo.SYSNAME",
            "dbo.NCHAR",
            "dbo.CHAR",
            "dbo.INT",
            "dbo.SMALLINT",
            "dbo.TINYINT",
            "DUMMY",
        };

        public static ICollection<string> SupportedTypes => Types;

        public static bool Supports(string typeName)
            => !string.IsNullOrEmpty(typeName) && SupportedTypes.Contains(typeName);
    }
}
