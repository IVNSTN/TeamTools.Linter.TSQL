using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0988", "INVALID_TYPE_SIZE")]
    internal sealed class InvalidTypeSizeRule : AbstractRule
    {
        private static readonly Dictionary<string, SizeRange> ValidSizeRanges;

        static InvalidTypeSizeRule()
        {
            ValidSizeRanges = new Dictionary<string, SizeRange>(StringComparer.OrdinalIgnoreCase)
            {
                { "VARCHAR", new SizeRange(1, 8000, true) },
                { "NVARCHAR", new SizeRange(1, 4000, true) },
                { "CHAR", new SizeRange(1, 8000) },
                { "NCHAR", new SizeRange(1, 4000) },
                { "BINARY", new SizeRange(1, 8000) },
                { "VARBINARY", new SizeRange(1, 8000, true) },
                { "DECIMAL", new SizeRange(1, 38) },
                { "NUMERIC", new SizeRange(1, 38) },
                { "DATETIME2", new SizeRange(0, 7) },
            };
        }

        public InvalidTypeSizeRule() : base()
        {
        }

        public override void Visit(SqlDataTypeReference node)
        {
            if (node.Name is null)
            {
                // e.g. cursor
                return;
            }

            if (node.Parameters.Count == 0)
            {
                // size/scale/precision omitted
                return;
            }

            string typeName = node.Name.BaseIdentifier.Value;

            if (!ValidSizeRanges.TryGetValue(typeName, out var validSizeRange))
            {
                // unsupported type
                return;
            }

            var declaredSize = node.Parameters[0];
            if (declaredSize is MaxLiteral && validSizeRange.MaxAllowed)
            {
                // e.g. VARCHAR(MAX)
                return;
            }

            if (!int.TryParse(declaredSize.Value, out int size))
            {
                HandleNodeError(node, Strings.ViolationDetails_InvalidTypeSizeRule_NotInteger);
            }

            if (size >= validSizeRange.Lo && size <= validSizeRange.Hi)
            {
                // size is valid
                return;
            }

            HandleNodeError(node, string.Format(
                Strings.ViolationDetails_InvalidTypeSizeRule_OutOfRange,
                size,
                validSizeRange.Lo,
                validSizeRange.Hi,
                typeName));
        }

        private sealed class SizeRange
        {
            public SizeRange(int lo, int hi, bool maxAllowed = false)
            {
                Lo = lo;
                Hi = hi;
                MaxAllowed = maxAllowed;
            }

            public int Lo { get; }

            public int Hi { get; }

            public bool MaxAllowed { get; }
        }
    }
}
