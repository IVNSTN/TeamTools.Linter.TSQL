using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0988", "INVALID_TYPE_SIZE")]
    internal sealed class InvalidTypeSizeRule : AbstractRule
    {
        private static readonly IDictionary<string, SizeRange> ValidSizeRanges = new SortedDictionary<string, SizeRange>(StringComparer.OrdinalIgnoreCase);

        static InvalidTypeSizeRule()
        {
            ValidSizeRanges.Add("VARCHAR", new SizeRange(1, 8000, true));
            ValidSizeRanges.Add("NVARCHAR", new SizeRange(1, 4000, true));
            ValidSizeRanges.Add("CHAR", new SizeRange(1, 8000));
            ValidSizeRanges.Add("NCHAR", new SizeRange(1, 4000));
            ValidSizeRanges.Add("BINARY", new SizeRange(1, 8000));
            ValidSizeRanges.Add("VARBINARY", new SizeRange(1, 8000, true));
            ValidSizeRanges.Add("DECIMAL", new SizeRange(1, 38));
            ValidSizeRanges.Add("NUMERIC", new SizeRange(1, 38));
            ValidSizeRanges.Add("DATETIME2", new SizeRange(0, 7));
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

            string typeName = node.Name.BaseIdentifier.Value;

            if (!ValidSizeRanges.ContainsKey(typeName))
            {
                // unsupported type
                return;
            }

            if (node.Parameters.Count == 0)
            {
                // size/scale/precision omitted
                return;
            }

            if (!int.TryParse(node.Parameters[0].Value, out int size))
            {
                if (node.Parameters[0] is MaxLiteral && ValidSizeRanges[typeName].MaxAllowed)
                {
                    // e.g. VARCHAR(MAX)
                    return;
                }

                HandleNodeError(node, "Size/precision is not a valid integer value");
            }

            if (size >= ValidSizeRanges[typeName].Lo && size <= ValidSizeRanges[typeName].Hi)
            {
                // size is valid
                return;
            }

            HandleNodeError(node, string.Format(
                "{3} size {0} is out of range {1}-{2}",
                size,
                ValidSizeRanges[typeName].Lo,
                ValidSizeRanges[typeName].Hi,
                typeName));
        }

        private struct SizeRange
        {
            public int Lo;
            public int Hi;
            public bool MaxAllowed;

            public SizeRange(int lo, int hi, bool maxAllowed = false)
            {
                Lo = lo;
                Hi = hi;
                MaxAllowed = maxAllowed;
            }
        }
    }
}
