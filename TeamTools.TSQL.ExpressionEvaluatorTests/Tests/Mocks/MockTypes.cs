using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public static class MockTypes
    {
        private static readonly ICollection<string> Types = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "VARCHAR",
            "NVARCHAR",
            "SYSNAME",
            "NCHAR",
            "CHAR",
            "INT",
            "SMALLINT",
            "TINYINT",
            "DUMMY",
        };

        public static ICollection<string> SupportedTypes => Types;

        public static bool Supports(string typeName)
            => !string.IsNullOrEmpty(typeName) && SupportedTypes.Contains(typeName);
    }
}
