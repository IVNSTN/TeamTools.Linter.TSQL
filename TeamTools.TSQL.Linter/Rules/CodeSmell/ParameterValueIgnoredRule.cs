using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0924", "PARAM_VALUE_IGNORED")]
    internal sealed class ParameterValueIgnoredRule : AbstractRule
    {
        public ParameterValueIgnoredRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBodyBase procOrFunc)
            {
                ValidateParams(procOrFunc.Parameters, procOrFunc.StatementList);
            }
        }

        private static IEnumerable<string> GetInputParams(IList<ProcedureParameter> parameters)
        {
            int n = parameters.Count;
            for (int i = 0; i < n; i++)
            {
                var p = parameters[i];
                if (!p.IsOutput() && !p.IsReadOnly())
                {
                    yield return p.VariableName.Value;
                }
            }
        }

        private void ValidateParams(IList<ProcedureParameter> parameters, TSqlFragment body)
        {
            if (body is null)
            {
                // CLR methods
                return;
            }

            if (parameters.Count == 0)
            {
                // no params to be ignored
                return;
            }

            var inputParams = new HashSet<string>(GetInputParams(parameters), StringComparer.OrdinalIgnoreCase);
            if (inputParams.Count == 0)
            {
                return;
            }

            var reportedParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var callback = new Action<TSqlFragment, string>((nd, varName) =>
            {
                // reporting only once per input param
                if (inputParams.Contains(varName) && reportedParams.Add(varName))
                {
                    HandleNodeError(nd, varName);
                }
            });

            var visitor = new VarRefsVisitor(callback);
            body.Accept(visitor);
        }

        private sealed class VarRefsVisitor : TSqlFragmentVisitor
        {
            private readonly HashSet<string> accessedVars = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private readonly Action<TSqlFragment, string> callback;
            private readonly VariableReferenceExtractor varRefExtractor;
            private int lastExpressionVisited = -1;

            public VarRefsVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
                varRefExtractor = new VariableReferenceExtractor(varRef => accessedVars.Add(varRef));
            }

            public override void Visit(SetVariableStatement node)
            {
                if (node.AssignmentKind != AssignmentKind.Equals)
                {
                    // ignoring @x += 1 and so on
                    return;
                }

                RegisterAccessedVars(node.Expression);

                if (accessedVars.Contains(node.Variable.Name))
                {
                    return;
                }

                OverwrittenVarDetected(node.Variable.Name, node.Variable);
            }

            // TODO : update-set, receive-set
            public override void Visit(SelectSetVariable node)
            {
                if (node.AssignmentKind != AssignmentKind.Equals)
                {
                    // ignoring @x += 1 and so on
                    return;
                }

                RegisterAccessedVars(node.Expression);

                if (accessedVars.Contains(node.Variable.Name))
                {
                    return;
                }

                OverwrittenVarDetected(node.Variable.Name, node.Variable);
            }

            public override void Visit(QuerySpecification node)
            {
                if (lastExpressionVisited > node.LastTokenIndex)
                {
                    return;
                }

                // clauses executed in this order
                // select with SelectSet happens after them all
                if (node.FromClause != null)
                {
                    RegisterAccessedVars(node.FromClause);
                }

                if (node.WhereClause != null)
                {
                    RegisterAccessedVars(node.WhereClause.SearchCondition);
                }

                if (node.GroupByClause != null)
                {
                    RegisterAccessedVars(node.GroupByClause.GroupingSpecifications);
                }

                if (node.HavingClause != null)
                {
                    RegisterAccessedVars(node.HavingClause.SearchCondition);
                }

                lastExpressionVisited = node.LastTokenIndex;
            }

            public override void Visit(ExecutableProcedureReference node)
            {
                int n = node.Parameters.Count;
                for (int i = 0; i < n; i++)
                {
                    if (node.Parameters[i].ParameterValue is VariableReference varRef)
                    {
                        accessedVars.Add(varRef.Name);
                    }
                }
            }

            public override void Visit(BooleanExpression node)
            {
                if (lastExpressionVisited > node.LastTokenIndex)
                {
                    return;
                }

                RegisterAccessedVars(node);

                lastExpressionVisited = node.LastTokenIndex;
            }

            public override void Visit(ScalarExpression node)
            {
                if (lastExpressionVisited > node.LastTokenIndex)
                {
                    return;
                }

                RegisterAccessedVars(node);

                lastExpressionVisited = node.LastTokenIndex;
            }

            public override void Visit(DeclareVariableElement node)
            {
                RegisterAccessedVars(node.Value);

                lastExpressionVisited = node.LastTokenIndex;
            }

            private void RegisterAccessedVars(IList<GroupingSpecification> groupings)
            {
                int n = groupings.Count;
                for (int i = 0; i < n; i++)
                {
                    if (groupings[i] is ExpressionGroupingSpecification gex)
                    {
                        RegisterAccessedVars(gex.Expression);
                    }
                }
            }

            private void RegisterAccessedVars(ScalarExpression expr) => expr?.Accept(varRefExtractor);

            private void RegisterAccessedVars(FromClause expr) => expr?.Accept(varRefExtractor);

            private void RegisterAccessedVars(BooleanExpression expr) => expr?.Accept(varRefExtractor);

            private void OverwrittenVarDetected(string varName, TSqlFragment node) => callback(node, varName);
        }

        private sealed class VariableReferenceExtractor : TSqlFragmentVisitor
        {
            private readonly Action<string> callback;

            public VariableReferenceExtractor(Action<string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(VariableReference node) => callback(node.Name);
        }
    }
}
