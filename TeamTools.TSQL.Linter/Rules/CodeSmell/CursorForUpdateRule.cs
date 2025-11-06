using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0745", "CURSOR_FOR_UPDATE")]
    [CursorRule]
    internal sealed class CursorForUpdateRule : BaseCursorDefinitionRule
    {
        public CursorForUpdateRule() : base()
        {
        }

        protected override void ValidateCursor(string cursorName, CursorDefinition node)
        {
            if (!(node.Select.QueryExpression.ForClause is UpdateForClause))
            {
                return;
            }

            if (node.Select.QueryExpression is QuerySpecification spec
            && (spec.FromClause?.TableReferences.Count ?? 0) < 2
            && spec.FromClause?.TableReferences[0] is NamedTableReference t)
            {
                string tableName = t.SchemaObject.BaseIdentifier.Value;

                if (tableName.StartsWith(TSqlDomainAttributes.TempTablePrefix)
                || tableName.StartsWith(TSqlDomainAttributes.VariablePrefix))
                {
                    // Single # or @ as a source is fine for row processing
                    return;
                }
            }

            HandleNodeError(node.Select.QueryExpression.ForClause, cursorName);
        }
    }
}
