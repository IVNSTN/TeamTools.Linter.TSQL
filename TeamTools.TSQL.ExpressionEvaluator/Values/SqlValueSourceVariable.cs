using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.ExpressionEvaluator.Values
{
    [ExcludeFromCodeCoverage]
    public sealed class SqlValueSourceVariable : SqlValueSource
    {
        public SqlValueSourceVariable(string varName, SqlValueSource origin, TSqlFragment node)
        : base(SqlValueSourceKind.Variable, node)
        {
            VarName = varName;
            Origin = origin;
        }

        public string VarName { get; }

        public SqlValueSource Origin { get; }

        public override SqlValueSource Clone() => new SqlValueSourceVariable(VarName, Origin, Node);

        public override sealed string ToString() => VarName;
    }
}
