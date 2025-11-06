using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class SqlValueSourceVariable : SqlValueSource
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

        public override string ToString() => VarName;
    }
}
