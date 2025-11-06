using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Xml;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0283", "INVALID_XML_LITERAL")]
    internal sealed class XmlValidSourceRule : AbstractRule
    {
        public XmlValidSourceRule() : base()
        {
        }

        public override void Visit(CreateXmlSchemaCollectionStatement node) => ValidateXmlExpression(node.Expression);

        public override void Visit(AlterXmlSchemaCollectionStatement node) => ValidateXmlExpression(node.Expression);

        public override void Visit(DeclareVariableElement node)
        {
            if (string.Equals(node.DataType.Name?.BaseIdentifier.Value, "XML", StringComparison.OrdinalIgnoreCase)
            && node.Value != null)
            {
                ValidateXmlExpression(node.Value);
            }
        }

        private static bool ValidateXml(string validatedText, out string error)
        {
            if (string.IsNullOrWhiteSpace(validatedText))
            {
                error = "";
                return true;
            }

            var validator = new XmlValidator();
            return validator.TryParse(validatedText, out error);
        }

        private void ValidateXmlExpression(ScalarExpression node)
        {
            if (node is StringLiteral src && !ValidateXml(src.Value, out string error))
            {
                HandleNodeError(node, error);
            }
        }

        private class XmlValidator : XmlDocument
        {
            public bool TryParse(string src, out string error)
            {
                error = "";

                try
                {
                    LoadXml(src);

                    return true;
                }
                catch (XmlException e)
                {
                    error = e.Message;
                }

                return false;
            }
        }
    }
}
