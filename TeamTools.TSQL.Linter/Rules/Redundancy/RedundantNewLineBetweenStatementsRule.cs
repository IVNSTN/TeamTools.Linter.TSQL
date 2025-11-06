using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // FIXME : extremely slow
    [RuleIdentity("RD0236", "REDUNDANT_NEWLINE")]
    internal sealed class RedundantNewLineBetweenStatementsRule : AbstractRule
    {
        // TODO : total refactoring
        private const int MaxLinesBreaksBetweenStatements = 2; // TODO : take from config

        private static readonly IList<TSqlTokenType> NonTSQLBlockTokens =
            new List<TSqlTokenType>
            {
                 TSqlTokenType.MultilineComment,
                 TSqlTokenType.SingleLineComment,
                 TSqlTokenType.Go,
            };

        private readonly char[] trimmedChars = new char[] { ';' };

        public RedundantNewLineBetweenStatementsRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var statementDetector = new StatementPositionDetector();
            node.Accept(statementDetector);

            DetectNonTSQLBlocks(node, statementDetector);

            ValidateNewLines(node.ScriptTokenStream, statementDetector.StatementPositions, statementDetector.StatementFirstTokens);
        }

        private void DetectNonTSQLBlocks(TSqlFragment node, StatementPositionDetector statementDetector)
        {
            // comments
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                if (NonTSQLBlockTokens.Contains(node.ScriptTokenStream[i].TokenType))
                {
                    int lineCount = node.ScriptTokenStream[i].Text.LineCount();
                    statementDetector.RegisterNonTSQLBlock(
                        node.ScriptTokenStream[i].Line,
                        node.ScriptTokenStream[i].Line + lineCount - 1,
                        node.ScriptTokenStream[i].TokenType,
                        i);
                }
            }
        }

        private void ValidateNewLines(
            IList<TSqlParserToken> scriptTokens,
            IDictionary<int, int> statementPositions,
            IDictionary<int, KeyValuePair<TSqlTokenType, int>> statementFirstTokens)
        {
            int lastBlockStartLine = 0;
            int lastBlockLastLine = 0;
            int maxLineBreaks;

            foreach (int blockStart in statementPositions.Keys)
            {
                if (blockStart < lastBlockLastLine)
                {
                    // nested block
                    lastBlockLastLine = lastBlockStartLine;
                }

                if (blockStart > lastBlockLastLine)
                {
                    int lineBreaks = blockStart - lastBlockLastLine;

                    if ((statementFirstTokens.ContainsKey(lastBlockStartLine) && statementFirstTokens[lastBlockStartLine].Key == TSqlTokenType.Else)
                    || (statementFirstTokens.ContainsKey(blockStart) && statementFirstTokens[blockStart].Key == TSqlTokenType.Else))
                    {
                        // no empty lines around ELSE
                        maxLineBreaks = 1;
                    }
                    else
                    if (statementFirstTokens.ContainsKey(lastBlockStartLine) && statementFirstTokens[lastBlockStartLine].Key == TSqlTokenType.Begin)
                    {
                        // no empty lines afer BEGIN
                        maxLineBreaks = 1;
                    }
                    else if (statementFirstTokens.ContainsKey(blockStart) && statementFirstTokens[blockStart].Key == TSqlTokenType.End)
                    {
                        // no empty lines before END
                        maxLineBreaks = 1;
                    }
                    else if (statementFirstTokens.ContainsKey(blockStart) && statementFirstTokens[blockStart].Key == TSqlTokenType.Variable)
                    {
                        // no empty lines before next proc/func parameter
                        maxLineBreaks = 1;
                    }
                    else
                    {
                        maxLineBreaks = MaxLinesBreaksBetweenStatements;
                    }

                    // TODO : really control LINE BREAKS, not empty lines
                    if (GetEmptyLinesBetween(scriptTokens, lastBlockLastLine, blockStart, statementFirstTokens) > maxLineBreaks)
                    {
                        HandleLineError(blockStart, 0);
                    }
                }

                lastBlockStartLine = blockStart;
                lastBlockLastLine = statementPositions[blockStart];
            }
        }

        private int GetEmptyLinesBetween(
            IList<TSqlParserToken> scriptTokens,
            int fromLine,
            int toLine,
            IDictionary<int, KeyValuePair<TSqlTokenType, int>> statementFirstTokens)
        {
            if (!statementFirstTokens.ContainsKey(fromLine) || !statementFirstTokens.ContainsKey(toLine))
            {
                return toLine - fromLine;
            }

            string textBetween = "";
            for (int i = statementFirstTokens[fromLine].Value; i < statementFirstTokens[toLine].Value; i++)
            {
                textBetween += scriptTokens[i].Text;
            }

            int emptyLinesBetween = 0;
            var linesBetween = textBetween.Split(Environment.NewLine);
            for (int i = linesBetween.Length - 1; i >= 0; i--)
            {
                // treating dangling semicolon as an empty line
                if (string.IsNullOrWhiteSpace(linesBetween[i].Trim(trimmedChars)))
                {
                    emptyLinesBetween++;
                }
                else
                {
                    break;
                }
            }

            return emptyLinesBetween;
        }

        private sealed class StatementPositionDetector : TSqlFragmentVisitor
        {
            private static readonly char[] TrimmedChars = new char[] { '\r', '\n', '\t', ' ', ';' };

            public SortedDictionary<int, KeyValuePair<TSqlTokenType, int>> StatementFirstTokens { get; }
                = new SortedDictionary<int, KeyValuePair<TSqlTokenType, int>>();

            public SortedDictionary<int, int> StatementPositions { get; } = new SortedDictionary<int, int>();

            public void RegisterNonTSQLBlock(int firstLine, int lastLine, TSqlTokenType tokenType, int tokenIndex)
            {
                if (StatementPositions.ContainsKey(firstLine))
                {
                    return;
                }

                int smallestBlockStart = -1;

                foreach (var block in StatementPositions)
                {
                    // if comment is in between other expression lines - do nothing
                    if (block.Key < firstLine && firstLine < block.Value)
                    {
                        // looking for deepest nested block
                        if (block.Key > smallestBlockStart)
                        {
                            smallestBlockStart = block.Key;
                        }
                    }
                }

                // except top-level blocks
                if (smallestBlockStart > -1 && StatementFirstTokens.ContainsKey(smallestBlockStart)
                && StatementFirstTokens[smallestBlockStart].Key != TSqlTokenType.Begin
                && StatementFirstTokens[smallestBlockStart].Key != TSqlTokenType.Create
                && StatementFirstTokens[smallestBlockStart].Key != TSqlTokenType.Alter)
                {
                    return;
                }

                StatementPositions.Add(firstLine, lastLine);
                StatementFirstTokens.Add(firstLine, new KeyValuePair<TSqlTokenType, int>(tokenType, tokenIndex));
            }

            public override void Visit(BeginEndBlockStatement node)
            {
                // register all Ends to avoid recursive analysis of registered blocks
                int tokenIndex = node.LastTokenIndex;

                // in case of whitespaces or semicolon
                while (node.ScriptTokenStream[tokenIndex].TokenType != TSqlTokenType.End)
                {
                    tokenIndex--;
                }

                RegisterCodeBlock(
                    node.ScriptTokenStream[tokenIndex].Line,
                    node.ScriptTokenStream[tokenIndex].Line,
                    TSqlTokenType.End,
                    tokenIndex);
            }

            public override void Visit(TryCatchStatement node)
            {
                // register all Ends to avoid recursive analysis of registered blocks
                int tokenIndex = node.CatchStatements.LastTokenIndex;

                // end catch
                // in case of whitespaces or semicolon
                while (node.ScriptTokenStream[tokenIndex].TokenType != TSqlTokenType.End)
                {
                    tokenIndex++;
                }

                RegisterCodeBlock(
                    node.ScriptTokenStream[tokenIndex].Line,
                    node.ScriptTokenStream[tokenIndex].Line,
                    TSqlTokenType.End,
                    tokenIndex);

                // ent try
                tokenIndex = node.TryStatements.LastTokenIndex;

                // in case of whitespaces or semicolon
                while (node.ScriptTokenStream[tokenIndex].TokenType != TSqlTokenType.End)
                {
                    tokenIndex++;
                }

                RegisterCodeBlock(
                    node.ScriptTokenStream[tokenIndex].Line,
                    node.ScriptTokenStream[tokenIndex].Line,
                    TSqlTokenType.End,
                    tokenIndex);
            }

            public override void Visit(IfStatement node)
            {
                // in IF, WHILE predicates can take multiple lines
                RegisterGenericCodeBlock(node.Predicate);

                // register all Elses because they are not separate statements
                // and otherwise'd be treated as empty lines
                if (null == node.ElseStatement)
                {
                    return;
                }

                int tokenIndex = node.ElseStatement.FirstTokenIndex;
                // because ElseStatement is the statement _after_ ELSE keyword
                while (node.ScriptTokenStream[tokenIndex].TokenType != TSqlTokenType.Else)
                {
                    tokenIndex--;
                }

                // register all Ends to avoid recursive analysis of registered blocks
                RegisterCodeBlock(
                    node.ScriptTokenStream[tokenIndex].Line,
                    node.ScriptTokenStream[tokenIndex].Line,
                    TSqlTokenType.Else,
                    tokenIndex);
            }

            // in IF, WHILE predicates can take multiple lines
            public override void Visit(WhileStatement node) => RegisterGenericCodeBlock(node.Predicate);

            public override void Visit(TSqlStatement node) => RegisterGenericCodeBlock(node);

            public override void Visit(ProcedureOption node) => RegisterGenericCodeBlock(node);

            public override void Visit(FunctionOption node) => RegisterGenericCodeBlock(node);

            public override void Visit(TriggerOption node) => RegisterGenericCodeBlock(node);

            public override void Visit(ProcedureParameter node) => RegisterGenericCodeBlock(node);

            public override void Visit(MethodSpecifier node) => RegisterGenericCodeBlock(node);

            private void RegisterCodeBlock(int firstLine, int lastLine, TSqlTokenType firstToken, int tokenIndex)
            {
                if (!StatementPositions.ContainsKey(firstLine))
                {
                    StatementPositions.Add(firstLine, lastLine);
                    if (firstToken != TSqlTokenType.None)
                    {
                        StatementFirstTokens.Add(firstLine, new KeyValuePair<TSqlTokenType, int>(firstToken, tokenIndex));
                    }
                }
                else if (StatementPositions[firstLine] < lastLine)
                {
                    StatementPositions[firstLine] = lastLine;
                }
            }

            private void RegisterGenericCodeBlock(TSqlFragment node)
            {
                // some parser bug
                if (node.FirstTokenIndex < 0)
                {
                    return;
                }

                string statementText = "";
                int start = node.FirstTokenIndex;
                int end = node.LastTokenIndex;

                for (int i = start; i <= end; i++)
                {
                    statementText += node.ScriptTokenStream[i].Text;
                }

                // semicolon may be prepended with many new lines but still treated
                // as a part of the (prior) statement
                statementText = statementText.TrimEnd(TrimmedChars);
                int lineCount = statementText.LineCount();

                TSqlTokenType firstTokenType = node.ScriptTokenStream[node.FirstTokenIndex].TokenType;

                // BEGIN dialog, BEGIN conversation and so on;
                if (!(node is BeginEndBlockStatement)
                && !(node is TryCatchStatement)
                && !(node is BeginEndAtomicBlockStatement)
                && (firstTokenType == TSqlTokenType.Begin))
                {
                    firstTokenType = TSqlTokenType.None; // whatever
                }

                RegisterCodeBlock(node.StartLine, node.StartLine + lineCount - 1, firstTokenType, node.FirstTokenIndex);
            }
        }
    }
}
