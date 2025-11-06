using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0936", "VAR_REF_BEFORE_ASSIGNED")]
    internal sealed class VariableNotAssignedBeforeUsageRule : AbstractRule
    {
        // very similar to AlwaysEmptyTableAsSourceRule
        public VariableNotAssignedBeforeUsageRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var detector = new VariableUsageDetector();
            node.AcceptChildren(detector);

            foreach (var badRef in detector.AccessingUnassignedVars)
            {
                HandleNodeError(badRef.Value, badRef.Key);
            }
        }

        private class VariableUsageDetector : TSqlFragmentVisitor
        {
            // key=var name, value=true if var assignment found
            private readonly IDictionary<string, bool> vars = new SortedDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            private readonly IDictionary<string, TSqlFragment> badRefs = new SortedDictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);
            private readonly IList<TSqlFragment> ignored = new List<TSqlFragment>();

            public IDictionary<string, TSqlFragment> AccessingUnassignedVars => badRefs;

            public override void Visit(DeclareVariableElement node)
            {
                // params are supposed to be passed (initialized) from outer scope
                if (node is ProcedureParameter)
                {
                    return;
                }

                if (vars.ContainsKey(node.VariableName.Value))
                {
                    return;
                }

                vars.Add(node.VariableName.Value, !IsNullExpression(node.Value));

                if (node.Value != null)
                {
                    // in case if there is an expression with references to other variables
                    node.Value.Accept(this);
                }
            }

            public override void Visit(VariableReference node)
            {
                if (badRefs.ContainsKey(node.Name) || ignored.Contains(node))
                {
                    return;
                }

                if (!vars.ContainsKey(node.Name) || vars[node.Name] == true)
                {
                    return;
                }

                badRefs.Add(node.Name, node);
            }

            public override void Visit(ExecutableProcedureReference node)
            {
                foreach (var prm in node.Parameters)
                {
                    if (prm.Variable != null)
                    {
                        // param names are also parsed as "variable references"
                        // ignoring them because they are initialized from the outside
                        ignored.Add(prm.Variable);
                    }

                    if (prm.IsOutput && prm.ParameterValue is VariableReference varRef)
                    {
                        // var passed as OUTPUT will be assigned in outer scope
                        MarkAssigned(varRef.Name);
                    }
                }
            }

            public override void Visit(SelectSetVariable node)
            {
                // in case if expression contains references to variables
                // expression is obviousely computed before assignment
                node.Expression?.Accept(this);

                // assignment to var by SELECT
                MarkAssigned(node.Variable.Name);
            }

            public override void Visit(AssignmentSetClause node)
            {
                // in case if expression contains references to variables
                // expression is obviousely computed before assignment
                node.NewValue?.Accept(this);

                // assignment to var insige UPDATE SET
                MarkAssigned(node.Variable?.Name);
            }

            public override void Visit(SetVariableStatement node)
            {
                // in case if expression contains references to variables
                // expression is obviousely computed before assignment
                node.Expression?.Accept(this);

                // assignment to var by SET
                MarkAssigned(node.Variable.Name);
            }

            public override void Visit(FetchCursorStatement node)
            {
                foreach (var fetchedVar in node.IntoVariables)
                {
                    MarkAssigned(fetchedVar.Name);
                }
            }

            // Filling table variable with INSERT
            public override void Visit(InsertSpecification node) => MarkFilled(node.Target);

            // Filling table variable with MERGE
            public override void Visit(MergeSpecification node) => MarkFilled(node.Target);

            // Filling table variable with OUTPUT INTO
            public override void Visit(OutputIntoClause node) => MarkFilled(node.IntoTable);

            // Filling table variable with RECEIVE INTO
            public override void Visit(ReceiveStatement node) => MarkAssigned(node.Into?.Variable.Name);

            // return value assigned to var
            public override void Visit(ExecuteSpecification node) => MarkAssigned(node.Variable?.Name);

            public override void Visit(VariableTableReference node) => MarkAssigned(node.Variable.Name);

            // Filling variable with GET CONVERSATION GROUP
            public override void Visit(GetConversationGroupStatement node) => MarkAssigned(node.GroupId?.Name);

            // Filling variable with BEGIN DIALOG CONVERSATION
            public override void Visit(BeginDialogStatement node) => MarkAssigned(node.Handle?.Name);

            private static bool IsNullExpression(ScalarExpression node)
            {
                while (node is ParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is null || node is NullLiteral)
                {
                    return true;
                }

                if (node is ScalarSubquery sel && sel.QueryExpression is QuerySpecification spec)
                {
                    if (spec.SelectElements.Count > 0 && spec.SelectElements[0] is SelectScalarExpression sclr)
                    {
                        return IsNullExpression(sclr.Expression);
                    }
                }

                return false;
            }

            private void MarkAssigned(string varName)
            {
                if (!string.IsNullOrEmpty(varName) && vars.ContainsKey(varName))
                {
                    vars[varName] = true;
                }
            }

            private void MarkFilled(TableReference tbl)
            {
                if (tbl is VariableTableReference tblVar)
                {
                    MarkAssigned(tblVar.Variable.Name);
                }
            }
        }
    }
}
