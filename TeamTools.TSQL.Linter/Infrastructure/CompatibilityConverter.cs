using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    public static class CompatibilityConverter
    {
        public static SqlVersion ToSqlVersion(int compatibilityLevel)
        {
            switch (compatibilityLevel)
            {
                case 90:
                    return SqlVersion.Sql90;
                case 100:
                    return SqlVersion.Sql100;
                case 110:
                    return SqlVersion.Sql110;
                case 130:
                    return SqlVersion.Sql130;
                case 140:
                    return SqlVersion.Sql140;
                case 150:
                    return SqlVersion.Sql150;
                case 160:
                    return SqlVersion.Sql160;
            }

            throw new ArgumentOutOfRangeException(
                nameof(compatibilityLevel),
                compatibilityLevel.ToString(),
                "Unsupported compatibility level");
        }
    }
}
