using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    public class ColumnVisitor : TSqlFragmentVisitor
    {
        private readonly ICollection<string> requiredTypes;

        public ColumnVisitor(ICollection<string> requiredTypes) : base()
        {
            this.requiredTypes = requiredTypes ?? throw new ArgumentNullException(nameof(requiredTypes));
        }

        public ICollection<string> Columns { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public override void Visit(ColumnDefinition node)
        {
            if (requiredTypes.Count > 0
            && !requiredTypes.Contains(node.DataType?.Name.BaseIdentifier.Value))
            {
                return;
            }

            Columns.Add(node.ColumnIdentifier.Value);
        }
    }
}
