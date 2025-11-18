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

        private readonly char[] trimmedChars = new char[] { ';' };

        public RedundantNewLineBetweenStatementsRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var statementDetector = new StatementPositionDetector();
            node.Accept(statementDetector);

            DetectNonTSQLBlocks(node, statementDetector);

            ValidateNewLines(node.ScriptTokenStream, statementDetector.StatementPositions, statementDetector.StatementFirstTokens);
        }

        private static bool IsNonTSQLBlockTokens(TSqlTokenType tokenType)
        {
            return tokenType == TSqlTokenType.MultilineComment
                || tokenType == TSqlTokenType.SingleLineComment
                || tokenType == TSqlTokenType.Go;
        }

        private static void DetectNonTSQLBlocks(TSqlFragment node, StatementPositionDetector statementDetector)
        {
            // comments
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (IsNonTSQLBlockTokens(token.TokenType))
                {
                    int lineCount = token.Text.LineCount();
                    statementDetector.RegisterNonTSQLBlock(
                        token.Line,
                        token.Line + lineCount - 1,
                        token.TokenType,
                        i);
                }
            }
        }

        private void ValidateNewLines(
            IList<TSqlParserToken> scriptTokens,
            IDictionary<int, int> statementPositions,
            IDictionary<int, Tuple<TSqlTokenType, int>> statementFirstTokens)
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

                    if ((statementFirstTokens.TryGetValue(lastBlockStartLine, out var lastBlockElse) && lastBlockElse.Item1 == TSqlTokenType.Else)
                    || (statementFirstTokens.TryGetValue(blockStart, out var blockStartElse) && blockStartElse.Item1 == TSqlTokenType.Else))
                    {
                        // no empty lines around ELSE
                        maxLineBreaks = 1;
                    }
                    else if (statementFirstTokens.TryGetValue(lastBlockStartLine, out var valueBegin) && valueBegin.Item1 == TSqlTokenType.Begin)
                    {
                        // no empty lines afer BEGIN
                        maxLineBreaks = 1;
                    }
                    else if (statementFirstTokens.TryGetValue(blockStart, out var valueEnd) && valueEnd.Item1 == TSqlTokenType.End)
                    {
                        // no empty lines before END
                        maxLineBreaks = 1;
                    }
                    else if (statementFirstTokens.TryGetValue(blockStart, out var valueVar) && valueVar.Item1 == TSqlTokenType.Variable)
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
            IDictionary<int, Tuple<TSqlTokenType, int>> statementFirstTokens)
        {
            if (!statementFirstTokens.TryGetValue(fromLine, out var fromLineValue) || !statementFirstTokens.TryGetValue(toLine, out var toLineValue))
            {
                return toLine - fromLine;
            }

            string textBetween = "";
            int n = toLineValue.Item2;
            for (int i = fromLineValue.Item2; i < n; i++)
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

            // They need to be sorted because we analyze them later in token position order
            public IDictionary<int, Tuple<TSqlTokenType, int>> StatementFirstTokens { get; }
                = new SortedDictionary<int, Tuple<TSqlTokenType, int>>();

            public IDictionary<int, int> StatementPositions { get; } = new SortedDictionary<int, int>();

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
                if (smallestBlockStart > -1
                && StatementFirstTokens.TryGetValue(smallestBlockStart, out var firstTokenValue)
                && !IsTopLevelStatementStart(firstTokenValue.Item1))
                {
                    return;
                }

                StatementPositions.Add(firstLine, lastLine);
                StatementFirstTokens.Add(firstLine, new Tuple<TSqlTokenType, int>(tokenType, tokenIndex));
            }

            public override void Visit(BeginEndBlockStatement node)
            {
                // register all Ends to avoid recursive analysis of registered blocks
                int endIndex = node.LastTokenIndex;
                var end = node.ScriptTokenStream[endIndex];

                // in case of trailing whitespaces, comment or semicolon
                while (end.TokenType != TSqlTokenType.End)
                {
                    end = node.ScriptTokenStream[--endIndex];
                }

                RegisterCodeBlock(
                    end.Line,
                    end.Line,
                    TSqlTokenType.End,
                    endIndex);
            }

            public override void Visit(TryCatchStatement node)
            {
                // register all Ends to avoid recursive analysis of registered blocks
                int endIndex = node.CatchStatements.LastTokenIndex;
                var end = node.ScriptTokenStream[endIndex];

                // end catch
                // in case of whitespaces or semicolon
                while (end.TokenType != TSqlTokenType.End)
                {
                    end = node.ScriptTokenStream[++endIndex];
                }

                RegisterCodeBlock(
                    end.Line,
                    end.Line,
                    TSqlTokenType.End,
                    endIndex);

                // ent try
                endIndex = node.TryStatements.LastTokenIndex;
                end = node.ScriptTokenStream[endIndex];

                // in case of whitespaces or semicolon
                while (end.TokenType != TSqlTokenType.End)
                {
                    end = node.ScriptTokenStream[++endIndex];
                }

                RegisterCodeBlock(
                    end.Line,
                    end.Line,
                    TSqlTokenType.End,
                    endIndex);
            }

            public override void Visit(IfStatement node)
            {
                // in IF, WHILE predicates can take multiple lines
                RegisterGenericCodeBlock(node.Predicate);

                // register all Elses because they are not separate statements
                // and otherwise'd be treated as empty lines
                if (node.ElseStatement is null)
                {
                    return;
                }

                int tokenIndex = node.ElseStatement.FirstTokenIndex;
                var elseToken = node.ScriptTokenStream[tokenIndex];

                // because ElseStatement is the statement _after_ ELSE keyword
                while (elseToken.TokenType != TSqlTokenType.Else)
                {
                    elseToken = node.ScriptTokenStream[--tokenIndex];
                }

                // register all Ends to avoid recursive analysis of registered blocks
                RegisterCodeBlock(
                    elseToken.Line,
                    elseToken.Line,
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

            private static bool IsTopLevelStatementStart(TSqlTokenType tokenType)
            {
                return tokenType == TSqlTokenType.Begin
                    || tokenType == TSqlTokenType.Create
                    || tokenType == TSqlTokenType.Alter;
            }

            private void RegisterCodeBlock(int firstLine, int lastLine, TSqlTokenType firstToken, int tokenIndex)
            {
                if (!StatementPositions.TryGetValue(firstLine, out var pos))
                {
                    StatementPositions.Add(firstLine, lastLine);
                    if (firstToken != TSqlTokenType.None)
                    {
                        StatementFirstTokens.Add(firstLine, new Tuple<TSqlTokenType, int>(firstToken, tokenIndex));
                    }
                }
                else if (pos < lastLine)
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
                int end = node.LastTokenIndex + 1;

                for (int i = start; i < end; i++)
                {
                    // TODO : use StringBuilder?
                    statementText = statementText + node.ScriptTokenStream[i].Text;
                }

                // TODO : avoid string manufacturing
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
