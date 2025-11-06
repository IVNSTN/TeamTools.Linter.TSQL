using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
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

        public override void Visit(TSqlBatch node) => node.AcceptChildren(new ConsecutiveAltersDetector(HandleNodeError));

        // TODO : very similar to DropStatementsIntoOneRule
        private class ConsecutiveAltersDetector : VisitorWithCallback
        {
            private static readonly int MaxItemsForOneStatement = 12;
            private readonly LastCmdInfo lastCmd = new LastCmdInfo();

            public ConsecutiveAltersDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(AlterTableAddTableElementStatement node) => SetLastCmd("ALTER-ADD", node.SchemaObjectName, node.Definition.ColumnDefinitions);

            public override void Visit(AlterTableDropTableElementStatement node) => SetLastCmd("ALTER-DROP", node.SchemaObjectName, node.AlterTableDropTableElements);

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

            private void SetLastCmd<T>(string cmd, SchemaObjectName name, IList<T> elements)
            where T : TSqlFragment
            {
                if (elements is null || elements.Count == 0)
                {
                    return;
                }

                string tableName = name.GetFullName();

                if (typeof(AlterTableDropTableElement).IsAssignableFrom(elements[0].GetType()))
                {
                    cmd += "-" + (elements[0] as AlterTableDropTableElement).TableElementType.ToString();
                }
                else if (typeof(ColumnDefinition).IsAssignableFrom(elements[0].GetType()))
                {
                    cmd += "-COL";
                }

                if (string.Equals(lastCmd.Command, cmd, StringComparison.OrdinalIgnoreCase)
                && string.Equals(lastCmd.TableName, tableName, StringComparison.OrdinalIgnoreCase)
                && (lastCmd.Elements + elements.Count) <= MaxItemsForOneStatement)
                {
                    Callback(name);
                }

                lastCmd.Command = cmd;
                lastCmd.TableName = tableName;
                lastCmd.Elements = elements.Count;
            }

            private void ResetLastCmd() => lastCmd.Command = "";
        }

        private class LastCmdInfo
        {
            public string Command { get; set; }

            public string TableName { get; set; }

            public int Elements { get; set; }
        }
    }
}
