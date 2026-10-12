using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.Linter.Properties;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class MultipleModificationAtomicRule
    {
        private class NoXactAbortDetector : BaseDmlDetector
        {
            private static readonly string Msg = Strings.ViolationDetails_MultipleModificationAtomicRule_NoXactAbort;

            public NoXactAbortDetector(Action<TSqlFragment, string> callback) : base(callback, Msg)
            { }

            private bool XactAbortEnabled { get; set; }

            // Ignoring all inside TRY-CATCH
            public override void ExplicitVisit(TryCatchStatement node)
            { }

            public override void Visit(PredicateSetStatement node)
            {
                if (node.Options.HasFlag(SetOptions.XactAbort))
                {
                    XactAbortEnabled = node.IsOn;
                }
            }

            protected override void WalkAllIfElseBranches(IfStatement node)
            {
                int dmlCountBeforeGoingNested = DmlCount;
                bool xactAbortBeforeGoingNested = XactAbortEnabled;
                int maxDmlCountAfterNested = 0;
                bool maxXactAbortAfterNested = false;

                var nestedIf = node.ThenStatement;
                var nestedElse = node.ElseStatement;

                while (nestedIf != null)
                {
                    DmlCount = dmlCountBeforeGoingNested;
                    XactAbortEnabled = xactAbortBeforeGoingNested;

                    // Going deeper
                    nestedIf.Accept(this);

                    if (DmlCount > maxDmlCountAfterNested)
                    {
                        maxDmlCountAfterNested = DmlCount;
                    }

                    if (XactAbortEnabled)
                    {
                        maxXactAbortAfterNested = XactAbortEnabled;
                    }

                    if (nestedElse is IfStatement elseIf)
                    {
                        nestedIf = elseIf.ThenStatement;
                        nestedElse = elseIf.ElseStatement;
                    }
                    else
                    {
                        nestedIf = nestedElse;
                        nestedElse = null;
                    }
                }

                DmlCount = maxDmlCountAfterNested;
                XactAbortEnabled = maxXactAbortAfterNested;
            }

            protected override bool CanDmlBeIgnored(DataModificationStatement node)
            {
                if (XactAbortEnabled)
                {
                    return true;
                }

                return base.CanDmlBeIgnored(node);
            }
        }
    }
}
