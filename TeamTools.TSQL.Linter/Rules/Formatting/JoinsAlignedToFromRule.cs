using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0220", "JOINS_ALIGN")]
    internal sealed class JoinsAlignedToFromRule : AbstractRule
    {
        private static readonly int MaxViolationsPerStatement = 2;
        private static readonly ICollection<TSqlTokenType> JoinTokenTypes;

        static JoinsAlignedToFromRule()
        {
            JoinTokenTypes = new List<TSqlTokenType>
            {
                TSqlTokenType.Join,
                TSqlTokenType.Right,
                TSqlTokenType.Left,
                TSqlTokenType.Inner,
                TSqlTokenType.Outer,
                TSqlTokenType.Full,
                TSqlTokenType.Cross,
            };
        }

        public JoinsAlignedToFromRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            if ((node.FromClause?.TableReferences?.Count ?? 0) == 0)
            {
                return;
            }

            if (node.StartLine == node.ScriptTokenStream[node.LastTokenIndex].Line)
            {
                // single line statements ignored
                return;
            }

            int requiredOffset = node.FromClause.StartColumn;
            var allJoins = ExtractJoinReferences(node.FromClause.TableReferences);

            ValidateJoins(allJoins, requiredOffset, HandleNodeError);
        }

        private static void ValidateJoins(IEnumerable<JoinTableReference> allJoins, int requiredOffset, Action<TSqlFragment> callback)
        {
            var badJoins = allJoins
                .Where(join => !IsValidJoinOffset(requiredOffset, join))
                .Take(MaxViolationsPerStatement);

            foreach (var join in badJoins)
            {
                callback(join);
            }
        }

        private static IEnumerable<JoinTableReference> ExtractJoinReferences(IList<TableReference> refs)
        {
            return refs
                .OfType<JoinTableReference>()
                .SelectMany(join => ExtractJoinReferences(join));
        }

        private static IEnumerable<JoinTableReference> ExtractJoinReferences(JoinTableReference join)
        {
            if (join.FirstTableReference is JoinTableReference firstJoin)
            {
                foreach (var nestedJoin in ExtractJoinReferences(firstJoin))
                {
                    yield return nestedJoin;
                }
            }

            if (join.SecondTableReference is JoinTableReference secondJoin)
            {
                foreach (var nestedJoin in ExtractJoinReferences(secondJoin))
                {
                    yield return nestedJoin;
                }
            }

            yield return join;
        }

        private static int GetJoinOffset(JoinTableReference node)
        {
            int start = node.FirstTableReference.LastTokenIndex + 1;
            int end = node.SecondTableReference.FirstTokenIndex;

            for (var i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];

                if (JoinTokenTypes.Contains(token.TokenType))
                {
                    return token.Column;
                }
            }

            return -1;
        }

        private static bool IsValidJoinOffset(int requiredOffset, JoinTableReference node)
        {
            int nodeOffset = GetJoinOffset(node);
            return (nodeOffset < 0) || (nodeOffset == requiredOffset);
        }
    }
}
