using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CompatibilityLevelAttribute : Attribute
    {
        public CompatibilityLevelAttribute(SqlVersion min, SqlVersion max)
        {
            MinVersion = min;
            MaxVersion = max;
        }

        public CompatibilityLevelAttribute(SqlVersion min)
        {
            MinVersion = min;
            MaxVersion = null;
        }

        public SqlVersion? MinVersion { get; }

        public SqlVersion? MaxVersion { get; }
    }
}
