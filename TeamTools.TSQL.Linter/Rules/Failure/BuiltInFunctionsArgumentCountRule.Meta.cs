using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Metadata initialization.
    /// </summary>
    internal partial class BuiltInFunctionsArgumentCountRule : ISqlServerMetadataConsumer
    {
        private static readonly Lazy<IList<TSqlTokenType>> TokenTypesInstance
            = new Lazy<IList<TSqlTokenType>>(() => InitTokenTypesInstance(), true);

        private IDictionary<string, SqlMetaProgrammabilitySignature> builtInFnArgCount;

        private static IList<TSqlTokenType> TokenTypes => TokenTypesInstance.Value;

        public void LoadMetadata(SqlServerMetadata data)
        {
            builtInFnArgCount = data.Functions;
        }

        private static IList<TSqlTokenType> InitTokenTypesInstance()
        {
            return new List<TSqlTokenType>
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
