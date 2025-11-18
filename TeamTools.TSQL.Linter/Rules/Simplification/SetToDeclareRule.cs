using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0735", "SET_TO_DECLARE")]
    internal sealed class SetToDeclareRule : AbstractRule
    {
        private static readonly int MaxStringLength = 32; // TODO : take limit from config
        private static readonly HashSet<string> AllowedFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "GETDATE",
            "SYSDATETIME",
        };

        private static readonly Func<TSqlStatement, IEnumerable<TSqlStatement>> ExtractionDelegate = new Func<TSqlStatement, IEnumerable<TSqlStatement>>(ExtractStatements);

        public SetToDeclareRule() : base()
        {
        }

        // TODO : take a look at ParameterValueIgnoredRule and refactor
        protected override void ValidateBatch(TSqlBatch node)
        {
            var declaredVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Action<string> removeVar = new Action<string>(varName =>
            {
                if (!string.IsNullOrEmpty(varName))
                {
                    declaredVariables.Remove(varName);
                }
            });

            var varRemover = new VarAssignementDetector(removeVar);

            // TODO : just visit needed statement, no recursion required
            foreach (var stmt in node.Statements.SelectMany(ExtractionDelegate))
            {
                if (stmt is GoToStatement || stmt is ReturnStatement || stmt is ThrowStatement)
                {
                    // considering this as the end
                    break;
                }
                else if (stmt is DeclareVariableStatement decl)
                {
                    int n = decl.Declarations.Count;
                    for (int i = 0; i < n; i++)
                    {
                        var d = decl.Declarations[i];
                        // except cursors and variables with value
                        if (d.DataType.Name != null && d.Value is null)
                        {
                            declaredVariables.Add(d.VariableName.Value);
                        }
                    }
                }
                else if (stmt is SetVariableStatement setVar)
                {
                    ValidateAssignment(setVar.Variable, setVar.Expression, declaredVariables, ViolationHandlerWithMessage);
                    // analyzing only the first assignment
                    declaredVariables.Remove(setVar.Variable.Name);
                }
                else if (stmt is SelectStatement sel)
                {
                    var spec = sel.QueryExpression.GetQuerySpecification();
                    var selected = ExtractSelectSetVar(spec?.SelectElements).ToArray();
                    bool isConditional = spec.FromClause != null || spec.WhereClause != null || spec.HavingClause != null;

                    if (selected != null && selected.Length > 0)
                    {
                        int n = selected.Length;
                        for (int i = 0; i < n; i++)
                        {
                            var selVar = selected[i];

                            if (!isConditional)
                            {
                                ValidateAssignment(selVar.Variable, selVar.Expression, declaredVariables, ViolationHandlerWithMessage);
                            }

                            // analyzing only the first assignment
                            declaredVariables.Remove(selVar.Variable.Name);
                        }
                    }
                }
                else
                {
                    stmt.Accept(varRemover);
                }
            }
        }

        private static void ValidateAssignment(VariableReference setVar, ScalarExpression expr, IEnumerable<string> declaredVariables, Action<TSqlFragment, string> callback)
        {
            if (declaredVariables.Contains(setVar.Name))
            {
                expr = expr.ExtractScalarExpression();

                if (expr is Literal l)
                {
                    // too long string literals are not welcome at declare
                    if (!(l is StringLiteral && l.Value.Length > MaxStringLength))
                    {
                        callback(setVar, setVar.Name);
                    }
                }
                else if (expr is FunctionCall fn && AllowedFunctions.Contains(fn.FunctionName.Value))
                {
                    callback(setVar, setVar.Name);
                }
            }
        }

        private static IEnumerable<SelectSetVariable> ExtractSelectSetVar(IList<SelectElement> sel)
        {
            if (sel is null)
            {
                yield break;
            }

            int n = sel.Count;
            for (int i = 0; i < n; i++)
            {
                if (sel[i] is SelectSetVariable setVar)
                {
                    yield return setVar;
                }
            }
        }

        private static IEnumerable<TSqlStatement> ExtractStatements(TSqlStatement node)
        {
            if (node is ProcedureStatementBodyBase proc)
            {
                return ExtractAll(proc.StatementList);
            }
            else if (node is TriggerStatementBody trg)
            {
                return ExtractAll(trg.StatementList);
            }
            else if (node is BeginEndBlockStatement be)
            {
                return ExtractAll(be.StatementList);
            }
            else if (node is TryCatchStatement tc)
            {
                // TODO : if there is a DML/exec before variable assignment then this can be treated
                // as a conditional assignment. Not sure if current implementation is correct for
                // TRY-CATCH body. Statements for CATCH are not needed for sure.
                return ExtractAll(tc.TryStatements);
            }
            else
            {
                return ExtractSelf(node);
            }
        }

        private static IEnumerable<TSqlStatement> ExtractAll(StatementList src) => src?.Statements?.SelectMany(ExtractionDelegate) ?? Enumerable.Empty<TSqlStatement>();

        private static IEnumerable<TSqlStatement> ExtractSelf(TSqlStatement node)
        {
            yield return node;
        }

        private static ScalarExpression ExtractExpression(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is UnaryExpression u)
            {
                return ExtractExpression(u.Expression);
            }

            return node;
        }

        private class VarAssignementDetector : TSqlFragmentVisitor
        {
            private readonly Action<string> callback;

            public VarAssignementDetector(Action<string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(SetVariableStatement node) => callback(node.Variable.Name);

            public override void Visit(SelectSetVariable node) => callback(node.Variable.Name);

            public override void Visit(ExecuteSpecification node) => callback(node.Variable?.Name);

            public override void Visit(ExecutableProcedureReference node)
            {
                int n = node.Parameters.Count;
                for (int i = 0; i < n; i++)
                {
                    var p = node.Parameters[i];
                    // When var is passed as OUTPUT parameter it means that called proc is supposed to set it
                    if (p.IsOutput && p.ParameterValue is VariableReference varRef)
                    {
                        callback(varRef.Name);
                    }
                }
            }
        }
    }
}
