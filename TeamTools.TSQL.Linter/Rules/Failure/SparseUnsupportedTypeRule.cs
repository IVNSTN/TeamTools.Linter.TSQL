using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0771", "SPARSE_UNSUPPORTED_TYPE")]
    internal sealed class SparseUnsupportedTypeRule : AbstractRule
    {
        private static readonly ICollection<string> UnsupportedTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TEXT",
            "NTEXT",
            "IMAGE",
            "TIMESTAMP",
            "ROWVERSION",
            "GEOGRAPHY",
            "GEOMETRY",
        };

        public SparseUnsupportedTypeRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            if (node.StorageOptions is null || node.StorageOptions.SparseOption == SparseColumnOption.None)
            {
                return;
            }

            string typeName = node.DataType?.Name.BaseIdentifier.Value;

            if (node.DataType is UserDataTypeReference
            || (!string.IsNullOrEmpty(typeName) && UnsupportedTypes.Contains(typeName)))
            {
                HandleNodeError(node.DataType, typeName);
            }
        }
    }
}
