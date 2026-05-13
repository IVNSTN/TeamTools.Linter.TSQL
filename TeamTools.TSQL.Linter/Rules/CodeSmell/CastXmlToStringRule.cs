using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0892", "CAST_XML_TO_STRING")]
    internal sealed class CastXmlToStringRule : AbstractRule
    {
        private static readonly string StuffFunction = "STUFF";

        private static readonly HashSet<string> TargetStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            TSqlDomainAttributes.Types.Char,
            TSqlDomainAttributes.Types.Varchar,
            TSqlDomainAttributes.Types.NChar,
            TSqlDomainAttributes.Types.NVarchar,
            TSqlDomainAttributes.Types.SysName,
        };

        public CastXmlToStringRule() : base()
        {
        }

        public override void Visit(CastCall node) => ValidateExpression(node.Parameter, node.DataType);

        public override void Visit(ConvertCall node) => ValidateExpression(node.Parameter, node.DataType);

        // STUFF(... FOR XML PATH('')), 1, 2, '') is a common solution for string concat
        public override void Visit(FunctionCall node)
        {
            if (!(node.FunctionName.Value.Equals(StuffFunction, StringComparison.OrdinalIgnoreCase)
                && node.Parameters.Count == 4))
            {
                return;
            }

            ValidateExpression(node.Parameters[0], null, TSqlDomainAttributes.Types.Varchar);
        }

        private static TSqlFragment ExtractForXmlQuery(ScalarExpression src)
        {
            while (src is ParenthesisExpression pe)
            {
                src = pe.Expression;
            }

            if (!(src is ScalarSubquery q && q.QueryExpression.ForClause is XmlForClause forXml))
            {
                return default;
            }

            for (int i = forXml.Options.Count - 1; i >= 0; i--)
            {
                var opt = forXml.Options[i];
                if (opt.OptionKind == XmlForClauseOptions.BinaryBase64)
                {
                    return default;
                }
                else if (opt.OptionKind == XmlForClauseOptions.Auto)
                {
                    return default;
                }
                else if (opt.OptionKind == XmlForClauseOptions.Path
                && opt.Value is StringLiteral s && string.IsNullOrEmpty(s.Value))
                {
                    // FOR XML PATH
                    return forXml;
                }
            }

            return default;
        }

        private void ValidateExpression(ScalarExpression src, DataTypeReference targetType, string targetTypeName = null)
        {
            var forXml = ExtractForXmlQuery(src);

            if (forXml is null)
            {
                // source is not SELECT FOR XML PATH
                return;
            }

            if (string.IsNullOrEmpty(targetTypeName))
            {
                targetTypeName = targetType.GetFullName();
            }

            if (!TargetStringTypes.Contains(targetTypeName))
            {
                // not a cast to string
                return;
            }

            HandleNodeError(forXml);
        }
    }
}
