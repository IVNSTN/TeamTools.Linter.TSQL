using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Visit(FunctionStatementBody node)
        {
            ValidateParams(node.Parameters, node.StatementList);
        }

        public override void Visit(ProcedureStatementBody node)
        {
            ValidateParams(node.Parameters, node.StatementList);
        }

        private void ValidateParams(IList<ProcedureParameter> parameters, TSqlFragment body)
        {
            if (body == null)
            {
                // CLR methods
                return;
            }

            var inputParams = parameters.Where(p => !p.IsOutput() && !p.IsReadOnly());

            if (!inputParams.Any())
            {
                return;
            }

            var overwrittenVars = GetVariablesAssignedBeforeReading(body);

            if (!overwrittenVars.Any())
            {
                return;
            }

            var ignoredParams = inputParams.Where(p => overwrittenVars.ContainsKey(p.VariableName.Value));

            if (!ignoredParams.Any())
            {
                return;
            }

            var ignoredParamNames = ignoredParams.Select(p => p.VariableName.Value).ToList();

            HandleNodeError(overwrittenVars[ignoredParamNames[0]], string.Join(", ", ignoredParamNames));
        }

        private IDictionary<string, TSqlFragment> GetVariablesAssignedBeforeReading(TSqlFragment body)
        {
            var visitor = new VarRefsVisitor();
            body.Accept(visitor);
            return visitor.OverwrittenVars;
        }

        private class VarRefsVisitor : TSqlFragmentVisitor
        {
            private readonly IList<string> accessedVars = new List<string>();
            private readonly IDictionary<string, TSqlFragment> overwrittenVars = new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);
            private readonly VariableReferenceExtractor varRefExtractor;

            public VarRefsVisitor()
            {
                varRefExtractor = new VariableReferenceExtractor(varRef => accessedVars.Add(varRef));
            }

            public IDictionary<string, TSqlFragment> OverwrittenVars => overwrittenVars;

            public override void Visit(SetVariableStatement node)
            {
                if (node.AssignmentKind != AssignmentKind.Equals)
                {
                    // ignoring @x += 1 and so on
                    return;
                }

                RegisterAccessedVars(node.Expression);

                if (accessedVars.Contains(node.Variable.Name, StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }

                if (overwrittenVars.ContainsKey(node.Variable.Name))
                {
                    return;
                }

                overwrittenVars.Add(node.Variable.Name, node.Variable);
            }

            public override void Visit(SelectSetVariable node)
            {
                if (node.AssignmentKind != AssignmentKind.Equals)
                {
                    // ignoring @x += 1 and so on
                    return;
                }

                RegisterAccessedVars(node.Expression);

                if (accessedVars.Contains(node.Variable.Name, StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }

                if (overwrittenVars.ContainsKey(node.Variable.Name))
                {
                    return;
                }

                overwrittenVars.Add(node.Variable.Name, node.Variable);
            }

            public override void Visit(QuerySpecification node)
            {
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
                    RegisterAccessedVars(
                        node.GroupByClause.GroupingSpecifications
                            .OfType<ExpressionGroupingSpecification>()
                            .Select(gr => gr.Expression));
                }

                if (node.HavingClause != null)
                {
                    RegisterAccessedVars(node.HavingClause.SearchCondition);
                }
            }

            public override void Visit(ExecutableProcedureReference node)
            {
                node.Parameters
                    .Select(p => p.ParameterValue)
                    .OfType<VariableReference>()
                    .ToList()
                    .ForEach(v => accessedVars.Add(v.Name));
            }

            public override void Visit(BooleanExpression node) => RegisterAccessedVars(node);

            public override void Visit(ScalarExpression node) => RegisterAccessedVars(node);

            public override void Visit(DeclareVariableElement node) => RegisterAccessedVars(node.Value);

            private void RegisterAccessedVars(ScalarExpression expr) => expr?.Accept(varRefExtractor);

            private void RegisterAccessedVars(IEnumerable<ScalarExpression> expr)
            {
                foreach (var e in expr)
                {
                    RegisterAccessedVars(e);
                }
            }

            private void RegisterAccessedVars(FromClause expr) => expr?.Accept(varRefExtractor);

            private void RegisterAccessedVars(BooleanExpression expr) => expr?.Accept(varRefExtractor);
        }

        private class VariableReferenceExtractor : TSqlFragmentVisitor
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
