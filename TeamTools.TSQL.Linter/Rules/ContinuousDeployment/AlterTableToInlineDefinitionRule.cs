using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0725", "ALTER_TO_DEFINITION")]
    internal sealed class AlterTableToInlineDefinitionRule : ScriptAnalysisServiceConsumingRule
    {
        public AlterTableToInlineDefinitionRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var mainObject = GetService<MainScriptObjectDetector>(node);
            if (string.IsNullOrWhiteSpace(mainObject?.ObjectFullName)
            || !(mainObject.ObjectDefinitionNode is CreateTableStatement))
            {
                return;
            }

            node.AcceptChildren(new AddConstraintVisitor(mainObject.ObjectFullName, ViolationHandlerWithMessage));
        }

        private sealed class AddConstraintVisitor : TSqlFragmentVisitor
        {
            private static readonly string MsgAvoidAlterConstraint = Strings.ViolationDetails_AlterTableToInlineDefinitionRule_Constraint;
            private static readonly string MsgAvoidAlterColumn = Strings.ViolationDetails_AlterTableToInlineDefinitionRule_Column;
            private static readonly string MsgAvoidDropTableElement = Strings.ViolationDetails_AlterTableToInlineDefinitionRule_Drop;

            private readonly string mainTableName;
            private readonly Action<TSqlFragment, string> callback;

            public AddConstraintVisitor(string mainTableName, Action<TSqlFragment, string> callback)
            {
                this.mainTableName = mainTableName;
                this.callback = callback;
            }

            public override void Visit(AlterTableAddTableElementStatement node)
            {
                if (!IsTheSameTable(node.SchemaObjectName))
                {
                    return;
                }

                if (node.Definition.TableConstraints.Count > 0)
                {
                    callback(node.Definition.TableConstraints[0].ConstraintIdentifier, MsgAvoidAlterConstraint);
                }

                if (node.Definition.ColumnDefinitions.Count > 0)
                {
                    callback(node.Definition.ColumnDefinitions[0].ColumnIdentifier, MsgAvoidAlterColumn);
                }
            }

            public override void Visit(AlterTableDropTableElementStatement node)
            {
                if (!IsTheSameTable(node.SchemaObjectName))
                {
                    return;
                }

                if (node.AlterTableDropTableElements.Count > 0)
                {
                    callback(node.AlterTableDropTableElements[0].Name, MsgAvoidDropTableElement);
                }
            }

            private bool IsTheSameTable(SchemaObjectName name)
                => string.Equals(name.GetFullName(), mainTableName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
