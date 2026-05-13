using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0890", "OBJECT_PROPERTY_FIDDLING")]
    internal sealed class ObjectPropertyFiddlingRule : AbstractRule
    {
        private static readonly HashSet<string> GetPropertyFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "OBJECTPROPERTY",
            "OBJECTPROPERTYEX",
            "COLUMNPROPERTY",
            "DATABASEPROPERTYEX",
            "FILEGROUPPROPERTY",
            "FILEPROPERTY",
            "FILEPROPERTYEX",
            "FULLTEXTCATALOGPROPERTY",
            "FULLTEXTSERVICEPROPERTY",
            "INDEXKEY_PROPERTY",
            "SERVERPROPERTY",
            "TYPEPROPERTY",
        };

        public ObjectPropertyFiddlingRule() : base()
        {
        }

        public override void Visit(FunctionCall node)
        {
            string funcName = node.FunctionName.Value;
            if (GetPropertyFunctions.Contains(funcName))
            {
                HandleNodeError(node, funcName);
            }
        }
    }
}
