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
            for (int i = 0, n = parameters.Count; i < n; i++)
            {
                var p = parameters[i];
                if (p.Modifier == ParameterModifier.None)
                {
                    // no OUTPUT or READONLY modifier - scalar input only parameter
                    yield return p.VariableName.Value;
                }
            }
        }

        private void ValidateParams(IList<ProcedureParameter> parameters, StatementList body)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                // CLR or empty body
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

            // SET @var = ...
            public override void Visit(SetVariableStatement node)
            {
                if (node.AssignmentKind != AssignmentKind.Equals)
                {
                    // ignoring @x += 1 and so on
                    return;
                }

                RegisterAccessedVars(node.Expression);
                DetectVarOverwriting(node.Variable);
            }

            // SELECT @var = ...
            // and RECEIVE @var = ...
            public override void Visit(SelectSetVariable node)
            {
                if (node.AssignmentKind != AssignmentKind.Equals)
                {
                    // ignoring @x += 1 and so on
                    return;
                }

                RegisterAccessedVars(node.Expression);
                DetectVarOverwriting(node.Variable);
            }

            // UPDATE t SET @var = ...
            public override void Visit(AssignmentSetClause node)
            {
                if (node.AssignmentKind != AssignmentKind.Equals)
                {
                    // ignoring @x += 1 and so on
                    return;
                }

                RegisterAccessedVars(node.NewValue);
                DetectVarOverwriting(node.Variable);
            }

            public override void Visit(QuerySpecification node)
            {
                if (lastExpressionVisited > node.LastTokenIndex)
                {
                    return;
                }

                // clauses executed in this order
                // select with SelectSet happens after them all
                RegisterAccessedVars(node.FromClause);
                RegisterAccessedVars(node.WhereClause?.SearchCondition);
                RegisterAccessedVars(node.GroupByClause?.GroupingSpecifications);
                RegisterAccessedVars(node.HavingClause?.SearchCondition);

                lastExpressionVisited = node.LastTokenIndex;
            }

            public override void Visit(ExecutableProcedureReference node)
            {
                for (int i = node.Parameters.Count - 1; i >= 0; i--)
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

            private void DetectVarOverwriting(VariableReference node)
            {
                if (node is null)
                {
                    return;
                }

                if (accessedVars.Contains(node.Name))
                {
                    // var value already accessed, overwriting is valid
                    return;
                }

                OverwrittenVarDetected(node.Name, node);
            }

            private void RegisterAccessedVars(IList<GroupingSpecification> groupings)
            {
                if (groupings is null)
                {
                    return;
                }

                for (int i = groupings.Count - 1; i >= 0; i--)
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
