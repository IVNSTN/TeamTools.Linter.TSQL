using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal abstract class BaseColumnLengthControlRule : AbstractRule
    {
        private static readonly int DefaultStringColumnSize = 30;
        private readonly int minSizeLimit;
        private readonly int maxSizeLimit;
        private readonly ICollection<string> supportedTypes;

        public BaseColumnLengthControlRule(int minSizeLimit, int maxSizeLimit, ICollection<string> supportedTypes) : base()
        {
            this.minSizeLimit = minSizeLimit;
            this.maxSizeLimit = maxSizeLimit;
            this.supportedTypes = supportedTypes;
        }

        public override void Visit(ColumnDefinition node)
        {
            string typeName = node.DataType?.Name.GetFullName();
            if (string.IsNullOrEmpty(typeName) || !supportedTypes.Contains(typeName))
            {
                return;
            }

            int definedSize = DefaultStringColumnSize;
            if (node.DataType is SqlDataTypeReference dt && dt.Parameters.Count == 1)
            {
                if (dt.Parameters[0] is MaxLiteral)
                {
                    definedSize = int.MaxValue;
                }
                else if (!(dt.Parameters[0] is IntegerLiteral sz && int.TryParse(sz.Value, out definedSize)))
                {
                    // something unparsable
                    return;
                }
            }

            if ((minSizeLimit > 0 && definedSize < minSizeLimit)
            || (maxSizeLimit > 0 && definedSize > maxSizeLimit))
            {
                HandleNodeError(node.DataType);
            }
        }
    }
}
