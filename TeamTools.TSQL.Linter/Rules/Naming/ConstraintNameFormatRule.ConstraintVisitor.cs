using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Constraint name validator.
    /// </summary>
    internal sealed partial class ConstraintNameFormatRule
    {
        private class ConstraintVisitor : TSqlFragmentVisitor
        {
            private static readonly string NamePartDelimiter = "_";
            private readonly ConstraintNameBuilder nameBuilder;
            private readonly Action<TSqlFragment, string> callback;
            private readonly string tableNameTemplatePart;
            private readonly Identifier contextColumn;

            public ConstraintVisitor(SchemaObjectName table, Identifier column, ConstraintNameBuilder nameBuilder, Action<TSqlFragment, string> callback) : base()
            {
                this.callback = callback;
                this.nameBuilder = nameBuilder;
                contextColumn = column;

                tableNameTemplatePart = table.BaseIdentifier.Value;
                if (string.IsNullOrEmpty(table.SchemaIdentifier?.Value) || string.Equals(table.SchemaIdentifier.Value, TSqlDomainAttributes.DefaultSchemaName, StringComparison.OrdinalIgnoreCase))
                {
                    tableNameTemplatePart = string.Format(
                        @"({0}{2})?{1}",
                        TSqlDomainAttributes.DefaultSchemaName,
                        table.BaseIdentifier.Value,
                        NamePartDelimiter);
                }
                else
                {
                    tableNameTemplatePart = $"{table.SchemaIdentifier.Value}{NamePartDelimiter}{table.BaseIdentifier.Value}";
                }
            }

            public override void Visit(CheckConstraintDefinition node)
            {
                if (node.ConstraintIdentifier is null)
                {
                    // ignoring noname constraint
                    return;
                }

                string expectedName = nameBuilder.Build(
                    ConstraintType.Check,
                    tableNameTemplatePart,
                    null);

                DoValidateConstraintName(node.ConstraintIdentifier, expectedName);
            }

            public override void Visit(DefaultConstraintDefinition node)
            {
                if (node.ConstraintIdentifier is null)
                {
                    // ignoring noname constraint
                    return;
                }

                if (contextColumn is null && node.Column is null)
                {
                    // to avoid double-check of inline column constraint
                    return;
                }

                string expectedName = nameBuilder.Build(
                    ConstraintType.Default,
                    tableNameTemplatePart,
                    new[] { contextColumn is null ? node.Column.Value : contextColumn.Value });

                DoValidateConstraintName(node.ConstraintIdentifier, expectedName);
            }

            public override void Visit(ForeignKeyConstraintDefinition node)
            {
                if (node.ConstraintIdentifier is null)
                {
                    // ignoring noname constraint
                    return;
                }

                if (contextColumn is null && ((node.Columns?.Count ?? 0) == 0))
                {
                    // to avoid double-check of inline column constraint
                    return;
                }

                string[] cols;
                if (contextColumn != null)
                {
                    cols = new[] { contextColumn.Value };
                }
                else
                {
                    cols = node.Columns.ExtractNames().ToArray();
                }

                string expectedName = nameBuilder.Build(
                    ConstraintType.ForeignKey,
                    tableNameTemplatePart,
                    cols);

                DoValidateConstraintName(node.ConstraintIdentifier, expectedName);
            }

            public override void Visit(UniqueConstraintDefinition node)
            {
                if (node.ConstraintIdentifier is null)
                {
                    // ignoring noname constraint
                    return;
                }

                if (contextColumn is null && ((node.Columns?.Count ?? 0) == 0))
                {
                    // to avoid double-check of inline column constraint
                    return;
                }

                string[] cols;
                if (contextColumn != null)
                {
                    cols = new[] { contextColumn.Value };
                }
                else
                {
                    cols = node.Columns.ExtractNames().ToArray();
                }

                string expectedName = nameBuilder.Build(
                    node.IsPrimaryKey ? ConstraintType.PrimaryKey : ConstraintType.Unique,
                    tableNameTemplatePart,
                    cols);

                DoValidateConstraintName(node.ConstraintIdentifier, expectedName);
            }

            private static bool IsValidName(string currentName, string expectedNamePattern)
            {
                Debug.Assert(currentName != "", "currentName is empty");
                Debug.Assert(expectedNamePattern != "", "expectedNamePattern is empty");
                Debug.WriteLine("comparing {0} vs {1}", currentName, expectedNamePattern);

                // TODO : letter casing should be checked to conform naming notation
                Regex r = new Regex(string.Format(@"^{0}$", expectedNamePattern), RegexOptions.Singleline | RegexOptions.Multiline);

                return r.IsMatch(currentName);
            }

            private void DoValidateConstraintName(Identifier currentName, string expectedNamePattern)
            {
                if (IsValidName(currentName.Value, expectedNamePattern))
                {
                    return;
                }

                callback(currentName, $"{currentName.Value} vs {expectedNamePattern}");
            }
        }
    }
}
