using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0276", "TWO_WORD_WITH_SINGLE_SPACE")]
    internal sealed class SingleSpaceInTwoWordInstructionRule : AbstractRule
    {
        private static readonly IList<TSqlTokenType> SeparatorTokens;
        private static readonly Regex TwoWordInstructionPattern = new Regex(
            "^([@\\w]+([\\s]{0,1})){2,}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        static SingleSpaceInTwoWordInstructionRule()
        {
            SeparatorTokens = new List<TSqlTokenType>
            {
                TSqlTokenType.WhiteSpace,
                TSqlTokenType.Semicolon,
                TSqlTokenType.SingleLineComment,
                TSqlTokenType.MultilineComment,
            };
        }

        public SingleSpaceInTwoWordInstructionRule() : base()
        {
        }

        public override void Visit(QualifiedJoin node)
        {
            int firstToken = node.FirstTableReference.LastTokenIndex + 1;
            while (node.ScriptTokenStream[firstToken].TokenType == TSqlTokenType.WhiteSpace)
            {
                firstToken++;
            }

            ValidateSpaceBetween(node, firstToken, node.SecondTableReference.FirstTokenIndex - 1);
        }

        public override void Visit(UnqualifiedJoin node)
        {
            int firstToken = node.FirstTableReference.LastTokenIndex + 1;
            while (node.ScriptTokenStream[firstToken].TokenType == TSqlTokenType.WhiteSpace)
            {
                firstToken++;
            }

            ValidateSpaceBetween(node, firstToken, node.SecondTableReference.FirstTokenIndex - 1);
        }

        public override void Visit(OrderByClause node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.OrderByElements[0].FirstTokenIndex - 1);

        public override void Visit(GroupByClause node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.GroupingSpecifications[0].FirstTokenIndex - 1);

        public override void Visit(DropTableStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1);

        public override void Visit(CreateTableStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.SchemaObjectName.FirstTokenIndex - 1);

        public override void Visit(AlterTableStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.SchemaObjectName.FirstTokenIndex - 1);

        public override void Visit(ProcedureStatementBody node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.ProcedureReference.FirstTokenIndex - 1);

        public override void Visit(DropProcedureStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1);

        public override void Visit(FunctionStatementBody node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1);

        public override void Visit(DropFunctionStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1);

        public override void Visit(TriggerStatementBody node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1);

        public override void Visit(DropTriggerStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1);

        public override void Visit(ViewStatementBody node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.SchemaObjectName.FirstTokenIndex - 1);

        public override void Visit(DropViewStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1);

        public override void Visit(CreateIndexStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1);

        public override void Visit(DropIndexStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.DropIndexClauses[0].FirstTokenIndex - 1);

        public override void Visit(CreateSynonymStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1);

        public override void Visit(DropSynonymStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1);

        public override void Visit(CreateTypeStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1);

        public override void Visit(DropTypeStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1);

        public override void Visit(OverClause node)
        {
            if (node.Partitions.Count == 0)
            {
                return;
            }

            int lastToken = node.Partitions[0].FirstTokenIndex - 1;
            int firstToken = lastToken;

            while (firstToken > 0 && !node.ScriptTokenStream[firstToken].Text.Equals("PARTITION", StringComparison.OrdinalIgnoreCase))
            {
                firstToken--;
            }

            ValidateSpaceBetween(node, firstToken, lastToken);
        }

        public override void Visit(TransactionStatement node)
        {
            if (node.Name is null)
            {
                ValidateSpaceBetween(node, node.FirstTokenIndex, node.LastTokenIndex);
                return;
            }

            int keywordPosition = node.Name.FirstTokenIndex;
            while (keywordPosition > node.FirstTokenIndex
                && node.ScriptTokenStream[keywordPosition].TokenType != TSqlTokenType.Tran
                && node.ScriptTokenStream[keywordPosition].TokenType != TSqlTokenType.Transaction)
            {
                keywordPosition--;
            }

            ValidateSpaceBetween(node, keywordPosition, node.Name.FirstTokenIndex);
            // in case if no TRANSACTION word
            if (keywordPosition > node.FirstTokenIndex)
            {
                ValidateSpaceBetween(node, node.FirstTokenIndex, keywordPosition);
            }
        }

        // TODO : refactoring needed
        public override void Visit(TryCatchStatement node)
        {
            // begin try
            {
                int firstToken = node.FirstTokenIndex;
                int end = node.LastTokenIndex;

                while (node.ScriptTokenStream[firstToken].TokenType != TSqlTokenType.Begin && firstToken < end)
                {
                    firstToken++;
                }

                int lastToken = firstToken + 1;

                while (!node.ScriptTokenStream[lastToken].Text.Equals("TRY", StringComparison.OrdinalIgnoreCase) && lastToken < end)
                {
                    lastToken++;
                }

                ValidateSpaceBetween(node, firstToken, lastToken);
            }

            // end try
            {
                int firstToken = node.TryStatements.LastTokenIndex;
                int end = node.LastTokenIndex;

                while (node.ScriptTokenStream[firstToken].TokenType != TSqlTokenType.End && firstToken < end)
                {
                    firstToken++;
                }

                int lastToken = firstToken + 1;

                while (!node.ScriptTokenStream[lastToken].Text.Equals("TRY", StringComparison.OrdinalIgnoreCase) && lastToken < end)
                {
                    lastToken++;
                }

                ValidateSpaceBetween(node, firstToken, lastToken);
            }

            // begin catch
            {
                int firstToken = node.TryStatements.LastTokenIndex;
                int end = node.LastTokenIndex;

                while (node.ScriptTokenStream[firstToken].TokenType != TSqlTokenType.Begin && firstToken < end)
                {
                    firstToken++;
                }

                int lastToken = firstToken + 1;

                while (!node.ScriptTokenStream[lastToken].Text.Equals("CATCH", StringComparison.OrdinalIgnoreCase) && lastToken < end)
                {
                    lastToken++;
                }

                ValidateSpaceBetween(node, firstToken, lastToken);
            }

            // end cath
            {
                int lastToken = node.LastTokenIndex;
                int end = node.LastTokenIndex;
                int start = node.FirstTokenIndex;

                while (!node.ScriptTokenStream[lastToken].Text.Equals("CATCH", StringComparison.OrdinalIgnoreCase) && lastToken > end)
                {
                    lastToken--;
                }

                int firstToken = lastToken - 1;

                while (node.ScriptTokenStream[firstToken].TokenType != TSqlTokenType.End && firstToken > start)
                {
                    firstToken--;
                }

                ValidateSpaceBetween(node, firstToken, lastToken);
            }
        }

        private void ValidateSpaceBetween(TSqlFragment node, int firstToken, int lastToken)
        {
            StringBuilder fragmenText = new StringBuilder();

            // fragment can include comments before and after, linebreaks and so on
            // which are not a part of validated expression
            while (lastToken > firstToken && SeparatorTokens.Contains(node.ScriptTokenStream[lastToken].TokenType))
            {
                lastToken--;
            }

            while (lastToken > firstToken && SeparatorTokens.Contains(node.ScriptTokenStream[firstToken].TokenType))
            {
                firstToken++;
            }

            for (int i = firstToken; i <= lastToken; i++)
            {
                fragmenText.Append(node.ScriptTokenStream[i].Text);
            }

            // TODO : the idea seems to be not quite right
            // + what about comments between words? or treating them as rule violation as well?
            if (TwoWordInstructionPattern.IsMatch(fragmenText.ToString()))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
