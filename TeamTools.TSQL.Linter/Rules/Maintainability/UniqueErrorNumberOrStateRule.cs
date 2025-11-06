using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0148", "UNIQUE_ERR_NO_OR_STATE")]

    internal class UniqueErrorNumberOrStateRule : AbstractRule
    {
        public UniqueErrorNumberOrStateRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
            => node.AcceptChildren(new ErrorVisitor(HandleNodeError));

        private class ErrorVisitor : VisitorWithCallback
        {
            public const int DefaultUserDefinedErrorNumber = 50000;
            private static readonly int MaxInfoSeverity = 10;
            private static readonly ICollection<string> StringTypes;

            private readonly IDictionary<string, string> declaredVariableTypes = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            static ErrorVisitor()
            {
                StringTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "VARCHAR",
                    "NVARCHAR",
                    "CHAR",
                    "NCHAR",
                };
            }

            public ErrorVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public ICollection<KeyValuePair<int, int>> ErrNumAndStates { get; } = new List<KeyValuePair<int, int>>();

            public override void Visit(DeclareVariableElement node)
            {
                if (node.DataType?.Name is null)
                {
                    // e.g. CURSOR
                    return;
                }

                if (declaredVariableTypes.ContainsKey(node.VariableName.Value))
                {
                    return;
                }

                string typeName = node.DataType.Name.BaseIdentifier.Value;
                if (StringTypes.Contains(typeName))
                {
                    // TODO : what is this replacement for?
                    typeName = "VARCHAR";
                }

                declaredVariableTypes.Add(node.VariableName.Value, typeName);
            }

            public override void Visit(RaiseErrorStatement node)
            {
                if (!ParseErrorNumberAndState(node.FirstParameter, node.ThirdParameter, out KeyValuePair<int, int> key))
                {
                    return;
                }

                if ((node.SecondParameter is IntegerLiteral severity)
                && int.Parse(severity.Value) <= MaxInfoSeverity)
                {
                    // ignoring info messages
                    return;
                }

                if (!ErrNumAndStates.TryAddUnique(key))
                {
                    Callback(node);
                }
            }

            public override void Visit(ThrowStatement node)
            {
                if (!ParseErrorNumberAndState(node.ErrorNumber, node.State, out KeyValuePair<int, int> key))
                {
                    return;
                }

                if (!ErrNumAndStates.TryAddUnique(key))
                {
                    Callback(node);
                }
            }

            protected bool ParseErrorNumberAndState(ScalarExpression errNum, ScalarExpression errState, out KeyValuePair<int, int> errInfo)
            {
                int errNumberValue;
                int errStateValue;
                string srcValue;
                errInfo = default;

                // we can say nothing about variable values
                if (!(errState is IntegerLiteral))
                {
                    return false;
                }

                srcValue = (errState as IntegerLiteral).Value;
                errStateValue = int.Parse(srcValue);

                // however if first argument is a string then the error number is 50000
                if (errNum is VariableReference varRef)
                {
                    string varName = varRef.Name;
                    if (declaredVariableTypes.ContainsKey(varName)
                    && declaredVariableTypes[varName].EndsWith("CHAR"))
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

                errInfo = new KeyValuePair<int, int>(errNumberValue, errStateValue);

                return true;
            }
        }
    }
}
