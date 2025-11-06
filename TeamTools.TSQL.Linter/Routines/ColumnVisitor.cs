using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    public class ColumnVisitor : TSqlFragmentVisitor
    {
        private readonly ICollection<string> requiredTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public ColumnVisitor(string[] requiredTypes = null) : base()
        {
            if (requiredTypes != null)
            {
                foreach (var typeName in requiredTypes)
                {
                    this.requiredTypes.TryAddUnique(typeName);
                }
            }
        }

        public IList<string> Columns { get; } = new List<string>();

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
