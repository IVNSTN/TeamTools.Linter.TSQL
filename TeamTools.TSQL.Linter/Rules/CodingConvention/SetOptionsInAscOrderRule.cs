using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0897", "SET_OPTIONS_ASC_ORDER")]
    internal sealed class SetOptionsInAscOrderRule : AbstractRule
    {
        private static readonly HashSet<string> Options = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ANSI_DEFAULTS",
            "ANSI_NULLS",
            "ANSI_NULL_DFLT_OFF",
            "ANSI_NULL_DFLT_ON",
            "ANSI_PADDING",
            "ANSI_WARNINGS",
            "ARITHABORT",
            "ARITHIGNORE",
            "CONCAT_NULL_YIELDS_NULL",
            "CURSOR_CLOSE_ON_COMMIT",
            "DISABLE_DEF_CNST_CHK",
            "FMTONLY",
            "FORCEPLAN",
            "IMPLICIT_TRANSACTIONS",
            "NOCOUNT",
            "NOEXEC",
            "NO_BROWSETABLE",
            "NUMERIC_ROUNDABORT",
            "PARSEONLY",
            "QUOTED_IDENTIFIER",
            "REMOTE_PROC_TRANSACTIONS",
            "SHOWPLAN_ALL",
            "SHOWPLAN_TEXT",
            "SHOWPLAN_XML",
            "XACT_ABORT",
        };

        public SetOptionsInAscOrderRule() : base()
        {
        }

        public override void Visit(PredicateSetStatement node)
        {
            string lastOptionName = null;

            for (int i = node.LastTokenIndex - 1, start = node.FirstTokenIndex; i >= start; i--)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.Identifier && Options.Contains(token.Text))
                {
                    if (lastOptionName != null
                    && string.Compare(lastOptionName, token.Text, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        HandleTokenError(token);
                    }

                    lastOptionName = token.Text;
                }
            }
        }
    }
}
