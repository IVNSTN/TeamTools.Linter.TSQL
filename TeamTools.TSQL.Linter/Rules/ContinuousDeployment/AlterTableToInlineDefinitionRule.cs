using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0725", "ALTER_TO_DEFINITION")]
    internal sealed class AlterTableToInlineDefinitionRule : AbstractRule
    {
        public AlterTableToInlineDefinitionRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var mainObjectDetector = new MainScriptObjectDetector();

            node.Accept(mainObjectDetector);

            if (!(mainObjectDetector.ObjectDefinitionNode is CreateTableStatement))
            {
                return;
            }

            node.AcceptChildren(new AddConstraintVisitor(mainObjectDetector.ObjectFullName, HandleNodeError));
        }

        private class AddConstraintVisitor : TSqlFragmentVisitor
        {
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

                ReportIfAny(node.Definition.TableConstraints.OfType<CheckConstraintDefinition>(), "check constraint");
                ReportIfAny(node.Definition.TableConstraints.OfType<DefaultConstraintDefinition>(), "default");
                ReportIfAny(node.Definition.TableConstraints.OfType<UniqueConstraintDefinition>(), "unique constraint");
                ReportIfAny(node.Definition.ColumnDefinitions, "column");
            }

            public override void Visit(AlterTableDropTableElementStatement node)
            {
                if (!IsTheSameTable(node.SchemaObjectName))
                {
                    return;
                }

                ReportIfAny(node.AlterTableDropTableElements, "don't drop - just remove element from definition");
            }

            private void ReportIfAny<T>(IEnumerable<T> elements, string msg)
            where T : TSqlFragment
            {
                var element = elements.FirstOrDefault();

                if (element != null)
                {
                    callback(element, msg);
                }
            }

            private bool IsTheSameTable(SchemaObjectName name)
                => string.Equals(name.GetFullName(), mainTableName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
