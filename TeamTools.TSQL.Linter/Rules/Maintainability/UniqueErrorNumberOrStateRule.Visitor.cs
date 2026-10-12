using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class UniqueErrorNumberOrStateRule : AbstractRule
    {
        private sealed class ErrorVisitor : VisitorWithCallback
        {
            public const int DefaultUserDefinedErrorNumber = 50000;
            private static readonly int MaxInfoSeverity = 10;
            private static readonly HashSet<string> StringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CHAR",
                "NCHAR",
                "NVARCHAR",
                "VARCHAR",
            };

            private HashSet<string> declaredStringVariables;
            private HashSet<ErrInfo> errNumAndStates;

            public ErrorVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public HashSet<ErrInfo> ErrNumAndStates => errNumAndStates ?? (errNumAndStates = new HashSet<ErrInfo>());

            public override void Visit(DeclareVariableElement node)
            {
                if (node.DataType?.Name is null)
                {
                    // e.g. CURSOR
                    return;
                }

                RegisterVar(node);
            }

            public override void Visit(RaiseErrorStatement node)
            {
                if (!ParseErrorNumberAndState(node.FirstParameter, node.ThirdParameter, out var err))
                {
                    return;
                }

                if ((node.SecondParameter is IntegerLiteral severity)
                && int.Parse(severity.Value) <= MaxInfoSeverity)
                {
                    // ignoring info messages
                    return;
                }

                ValidateUniqueness(err, node);
            }

            public override void Visit(ThrowStatement node)
            {
                if (!ParseErrorNumberAndState(node.ErrorNumber, node.State, out var err))
                {
                    return;
                }

                ValidateUniqueness(err, node);
            }

            private bool ParseErrorNumberAndState(ScalarExpression errNum, ScalarExpression errState, out ErrInfo errInfo)
            {
                int errNumberValue;
                int errStateValue;
                string srcValue;
                errInfo = default;

                // we can say nothing about variable values
                if (!(errState is IntegerLiteral i))
                {
                    return false;
                }

                srcValue = i.Value;
                errStateValue = int.Parse(srcValue);

                // however if first argument is a string then the error number is 50000
                if (errNum is VariableReference varRef)
                {
                    string varName = varRef.Name;
                    if (declaredStringVariables?.Contains(varName) ?? false)
                    {
                        // first parameter is a message
                        errNumberValue = DefaultUserDefinedErrorNumber;
                    }
                    else
                    {
                        // if int variable then we don't know which error number is that
                        return false;
                    }
                }
                else if (errNum is IntegerLiteral intValue)
                {
                    if (!int.TryParse(intValue.Value, out errNumberValue))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                errInfo = new ErrInfo(errNumberValue, errStateValue);

                return true;
            }

            private void ValidateUniqueness(ErrInfo errInfo, TSqlFragment node)
            {
                if (!ErrNumAndStates.Add(errInfo))
                {
                    Callback(node);
                }
            }

            private void RegisterVar(DeclareVariableElement node)
            {
                if (declaredStringVariables?.Contains(node.VariableName.Value) ?? false)
                {
                    return;
                }

                string typeName = node.DataType.Name.BaseIdentifier.Value;
                if (!StringTypes.Contains(typeName))
                {
                    // storing only char-typed vars since we don't need others
                    return;
                }

                (declaredStringVariables ?? (declaredStringVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase))).Add(node.VariableName.Value);
            }
        }
    }
}
