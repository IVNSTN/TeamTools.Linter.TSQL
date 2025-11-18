using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0754", "ALTER_STATEMENTS_INTO_ONE")]
    internal sealed class AlterStatementsIntoOneRule : AbstractRule
    {
        public AlterStatementsIntoOneRule() : base()
        {
        }

        private enum AlterKind
        {
            None = 0,
            AlterAdd,
            AlterDrop,
        }

        protected override void ValidateScript(TSqlScript node) => node.Accept(new ConsecutiveAltersDetector(ViolationHandler));

        private struct LastCmdInfo
        {
            public string TableName { get; set; }

            public AlterKind AlterType { get; set; }

            public TableElementType ElementsType { get; set; }

            public int ElementCount { get; set; }
        }

        // TODO : very similar to DropStatementsIntoOneRule
        private class ConsecutiveAltersDetector : VisitorWithCallback
        {
            private static readonly int MaxItemsForOneStatement = 12;
            private LastCmdInfo lastCmd = new LastCmdInfo();

            public ConsecutiveAltersDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(AlterTableAddTableElementStatement node) => SetLastCmd(
                node.SchemaObjectName,
                AlterKind.AlterAdd,
                node.Definition.ColumnDefinitions.Count > 0 ? TableElementType.Column : TableElementType.Constraint,
                Math.Max(node.Definition.ColumnDefinitions.Count, node.Definition.TableConstraints.Count));

            public override void Visit(AlterTableDropTableElementStatement node) => SetLastCmd(node.SchemaObjectName, AlterKind.AlterDrop, node.AlterTableDropTableElements[0].TableElementType, node.AlterTableDropTableElements.Count);

            public override void Visit(TSqlStatement node)
            {
                // Alter-statements are handled by other methods
                // declarations and set-var don't seem to be logic breakers
                if (node is AlterTableDropTableElementStatement
                || node is AlterTableAddTableElementStatement
                || node is SetVariableStatement
                || node is DeclareVariableStatement
                || node is DeclareTableVariableStatement)
                {
                    return;
                }

                ResetLastCmd();
            }

            private void SetLastCmd(SchemaObjectName name, AlterKind alterKind, TableElementType elementType, int elementCount)
            {
                string tableName = name.GetFullName();

                if (elementCount < MaxItemsForOneStatement
                && lastCmd.ElementCount < MaxItemsForOneStatement
                && lastCmd.AlterType == alterKind
                && lastCmd.ElementsType == elementType
                && lastCmd.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                {
                    Callback(name);
                }

                lastCmd.AlterType = alterKind;
                lastCmd.TableName = tableName;
                lastCmd.ElementsType = elementType;
                lastCmd.ElementCount = elementCount;
            }

            private void ResetLastCmd() => lastCmd.AlterType = AlterKind.None;
        }
    }
}
