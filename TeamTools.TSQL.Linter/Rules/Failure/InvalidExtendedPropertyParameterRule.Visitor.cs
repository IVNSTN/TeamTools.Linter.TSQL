using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class InvalidExtendedPropertyParameterRule
    {
        private sealed class ExtendedPropertyValidator : ExtendedPropertyEditingVisitor
        {
            // docs: https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-addextendedproperty-transact-sql?view=sql-server-ver17#arguments
            private static readonly HashSet<string> Level0Types = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ASSEMBLY",
                "CONTRACT",
                "EVENT NOTIFICATION",
                "FILEGROUP",
                "MESSAGE TYPE",
                "PARTITION FUNCTION",
                "PARTITION SCHEME",
                "REMOTE SERVICE BINDING",
                "ROUTE",
                "SCHEMA",
                "SERVICE",
                "USER",
                "TRIGGER",
                "TYPE",
                "PLAN GUIDE",
            };

            private static readonly HashSet<string> Level1Types = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "AGGREGATE",
                "DEFAULT",
                "FUNCTION",
                "LOGICAL FILE NAME",
                "PROCEDURE",
                "QUEUE",
                "RULE",
                "SEQUENCE",
                "SYNONYM",
                "TABLE",
                "TABLE_TYPE",
                "TYPE",
                "VIEW",
                "XML SCHEMA COLLECTION",
            };

            private static readonly HashSet<string> Level2Types = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "COLUMN",
                "CONSTRAINT",
                "EVENT NOTIFICATION",
                "INDEX",
                "PARAMETER",
                "TRIGGER",
            };

            private static readonly Dictionary<string, HashSet<string>> LevelArgTypeMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "@level0type", Level0Types },
                { "@level1type", Level1Types },
                { "@level2type", Level2Types },
            };

            public ExtendedPropertyValidator(Action<TSqlFragment, string> callback) : base(callback)
            { }

            // Only named EXEC parameter provisioning is supported.
            // There is a separate rule for preventing passing arguments by position.
            protected override void ValidatePropertyEditingProcArgs(IList<ExecuteParameter> procParams, TSqlFragment call)
            {
                for (int i = procParams.Count - 1; i >= 0; i--)
                {
                    var param = procParams[i];
                    if (param.Variable != null
                    && param.ParameterValue is StringLiteral levelTypeValue
                    && LevelArgTypeMap.TryGetValue(param.Variable.Name, out var validLevelTypes))
                    {
                        if (!validLevelTypes.Contains(levelTypeValue.Value))
                        {
                            Callback(param, levelTypeValue.Value);
                        }
                    }
                }
            }
        }
    }
}
