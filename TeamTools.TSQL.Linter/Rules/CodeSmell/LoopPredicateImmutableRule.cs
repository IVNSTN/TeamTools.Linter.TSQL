using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0940", "LOOP_PREDICATE_IMMUTABLE")]
    internal sealed class LoopPredicateImmutableRule : AbstractRule
    {
        public LoopPredicateImmutableRule() : base()
        {
        }

        public override void Visit(WhileStatement node)
        {
            var modificationVisitor = new VariableChangeVisitor();
            node.Statement.Accept(modificationVisitor);

            // TODO : support @@FETCH_STATUS, @@ROWCOUNT, @@TRANCOUNT and operations changing their value
            // TODO : support CAST, SUBSTRING, ROUND and other functions performed over immutable variables
            var immutableVars = ExtractImmutablePredicateVars(node.Predicate, modificationVisitor.VariablesWithModifications);
            if (!immutableVars.Any())
            {
                return;
            }

            var varNames = immutableVars
                .Select(v => v.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v);

            HandleNodeError(immutableVars.First(), string.Join(", ", varNames));
        }

        private static bool IsVarAndLiteralOnlyExpression(ScalarExpression node, IList<VariableReference> variables)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is BinaryExpression bin)
            {
                return IsVarAndLiteralOnlyExpression(bin.FirstExpression, variables)
                    && IsVarAndLiteralOnlyExpression(bin.SecondExpression, variables);
            }

            if (node is Literal)
            {
                return true;
            }

            if (node is VariableReference vref)
            {
                variables.Add(vref);
                return true;
            }

            return false;
        }

        private IEnumerable<VariableReference> ExtractImmutablePredicateVars(BooleanExpression predicate, ICollection<string> mutatedVars)
        {
            while (predicate is BooleanParenthesisExpression pe)
            {
                predicate = pe.Expression;
            }

            if (predicate is BooleanBinaryExpression bin)
            {
                return ExtractImmutablePredicateVars(bin.FirstExpression, mutatedVars)
                    .Union(ExtractImmutablePredicateVars(bin.SecondExpression, mutatedVars));
            }

            if (predicate is BooleanTernaryExpression ternary)
            {
                var extractedVars = new List<VariableReference>();

                if (IsVarAndLiteralOnlyExpression(ternary.FirstExpression, extractedVars)
                   && IsVarAndLiteralOnlyExpression(ternary.SecondExpression, extractedVars)
                   && IsVarAndLiteralOnlyExpression(ternary.ThirdExpression, extractedVars)
                   && !extractedVars.Any(v => mutatedVars.Contains(v.Name)))
                {
                    return extractedVars;
                }
            }
            else if (predicate is BooleanComparisonExpression cmp)
            {
                var extractedVars = new List<VariableReference>();

                if (IsVarAndLiteralOnlyExpression(cmp.FirstExpression, extractedVars)
                    && IsVarAndLiteralOnlyExpression(cmp.SecondExpression, extractedVars)
                    && !extractedVars.Any(v => mutatedVars.Contains(v.Name)))
                {
                    return extractedVars;
                }
            }

            return Enumerable.Empty<VariableReference>();
        }

        private sealed class VariableChangeVisitor : TSqlFragmentVisitor
        {
            public HashSet<string> VariablesWithModifications { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public override void Visit(SetVariableStatement node) => RegisterVariable(node.Variable.Name);

            public override void Visit(SelectSetVariable node) => RegisterVariable(node.Variable.Name);

            // EXEC @res = ...
            public override void Visit(ExecuteSpecification node) => RegisterVariable(node.Variable?.Name);

            public override void Visit(ExecutableProcedureReference node)
            {
                int n = node.Parameters.Count;
                for (int i = 0; i < n; i++)
                {
                    var p = node.Parameters[i];
                    if (p.IsOutput && p.ParameterValue != null
                    && p.ParameterValue is VariableReference varRef)
                    {
                        RegisterVariable(varRef.Name);
                    }
                }
            }

            private void RegisterVariable(string variableName)
            {
                if (!string.IsNullOrEmpty(variableName))
                {
                    VariablesWithModifications.Add(variableName);
                }
            }
        }
    }
}
