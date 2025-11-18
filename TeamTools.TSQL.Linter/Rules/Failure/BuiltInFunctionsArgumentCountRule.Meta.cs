using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Infrastructure;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Metadata initialization.
    /// </summary>
    internal partial class BuiltInFunctionsArgumentCountRule : ISqlServerMetadataConsumer
    {
        private static readonly Lazy<HashSet<TSqlTokenType>> TokenTypesInstance
            = new Lazy<HashSet<TSqlTokenType>>(() => InitTokenTypesInstance(), true);

        private Dictionary<string, SqlServerMetaProgrammabilitySignature> builtInFnArgCount;

        private static HashSet<TSqlTokenType> TokenTypes => TokenTypesInstance.Value;

        public void LoadMetadata(SqlServerMetadata data)
        {
            builtInFnArgCount = new Dictionary<string, SqlServerMetaProgrammabilitySignature>(data.Functions, StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<TSqlTokenType> InitTokenTypesInstance()
        {
            return new HashSet<TSqlTokenType>
            {
                TSqlTokenType.Identifier,
                TSqlTokenType.Left,
                TSqlTokenType.Right,
                TSqlTokenType.NullIf,
                TSqlTokenType.SystemUser,
                TSqlTokenType.CurrentTimestamp,
            };
        }
    }
}
