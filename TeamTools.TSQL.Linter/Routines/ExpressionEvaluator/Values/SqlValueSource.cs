using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class SqlValueSource
    {
        public SqlValueSource(SqlValueSourceKind sourceKind, TSqlFragment node)
        {
            SourceKind = sourceKind;
            Node = node;
        }

        public SqlValueSourceKind SourceKind { get; }

        public TSqlFragment Node { get; }

        public virtual SqlValueSource Clone() => new SqlValueSource(SourceKind, Node);

        public override string ToString()
        {
            switch (SourceKind)
            {
                case SqlValueSourceKind.Literal:
                    return "Literal";
                case SqlValueSourceKind.Variable:
                    return "Variable";
                default:
                    return "Expression";
            }
        }
    }
}
