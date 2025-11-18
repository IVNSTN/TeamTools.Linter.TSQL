using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : math functions
    // TODO : CHOOSE almost same as COALESCE
    // TODO : NULL literal can be treated as INT (in ISNULL, select output) or as UNKNOWN (in COALESCE)
    // TODO : use Int.TryParse to check if literal can be converted to INT
    // TODO : try to parse literal to check if it can be converted to DATE/TIME types; be aware of many possible formats
    // TODO : XML.value(, 'DATATYPE')
    // TODO : validate CAST, CONVERT calls for source and target type compatibility
    // note, some _explicit_ conversions can succeed even if implicit conversion is impossible
    // this means that new compatibility map is required
    [RuleIdentity("FA0948", "ILLEGAL_IMPLICIT_CONVERSION")]
    internal sealed class ImplicitConversionImpossibleRule : AbstractRule, IDynamicSqlParser, ISqlServerMetadataConsumer
    {
        private static readonly Dictionary<string, HashSet<string>> Impossible = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> NoImplicitConversionTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> knownReturnTypes;
        private TSqlParser parser;
        private SqlServerMetadata metaData;

        static ImplicitConversionImpossibleRule()
        {
            // TODO: turn inside out? register only possible conversions?
            MakeImpossible("BINARY", "DATE");
            MakeImpossible("BINARY", "TIME");
            MakeImpossible("BINARY", "DATETIMEOFFSET");
            MakeImpossible("BINARY", "DATETIME2");
            MakeImpossible("BINARY", "FLOAT");
            MakeImpossible("BINARY", "REAL");
            MakeImpossible("BINARY", "TEXT");
            MakeImpossible("BINARY", "NTEXT");

            MakeImpossibleSameAs("VARBINARY", "BINARY");

            MakeImpossible("CHAR", "BINARY");
            MakeImpossible("CHAR", "VARBINARY");
            MakeImpossible("CHAR", "TIMESTAMP");
            MakeImpossible("CHAR", "VARBINARY");

            MakeImpossibleSameAs("VARCHAR", "CHAR");
            MakeImpossibleSameAs("NCHAR", "CHAR");
            MakeImpossibleSameAs("NVARCHAR", "CHAR");
            MakeImpossible("NCHAR", "IMAGE");
            MakeImpossible("NVARCHAR", "IMAGE");

            MakeImpossible("DATETIME", "BINARY");
            MakeImpossible("DATETIME", "VARBINARY");
            MakeImpossible("DATETIME", "FLOAT");
            MakeImpossible("DATETIME", "REAL");
            MakeImpossible("DATETIME", "DECIMAL");
            MakeImpossible("DATETIME", "BIGINT");
            MakeImpossible("DATETIME", "INT");
            MakeImpossible("DATETIME", "SMALLINT");
            MakeImpossible("DATETIME", "TINYINT");
            MakeImpossible("DATETIME", "MONEY");
            MakeImpossible("DATETIME", "SMALLMONEY");
            MakeImpossible("DATETIME", "BIT");
            MakeImpossible("DATETIME", "TIMESTAMP");
            MakeImpossible("DATETIME", "UNIQUEIDENTIFIER");
            MakeImpossible("DATETIME", "IMAGE");
            MakeImpossible("DATETIME", "TEXT");
            MakeImpossible("DATETIME", "NTEXT");
            MakeImpossible("DATETIME", "XML");
            MakeImpossible("DATETIME", "HIERARCHYID");

            MakeImpossibleSameAs("DATETIME2", "DATETIME");
            MakeImpossibleSameAs("DATETIMEOFFSET", "DATETIME");
            MakeImpossibleSameAs("SMALLDATETIME", "DATETIME");
            MakeImpossibleSameAs("DATE", "DATETIME");
            MakeImpossible("DATE", "TIME");
            MakeImpossibleSameAs("TIME", "DATETIME");
            MakeImpossible("TIME", "DATE");

            MakeImpossible("DECIMAL", "DATE");
            MakeImpossible("DECIMAL", "TIME");
            MakeImpossible("DECIMAL", "DATETIME2");
            MakeImpossible("DECIMAL", "DATETIMEOFFSET");
            MakeImpossible("DECIMAL", "UNIQUEIDENTIFIER");
            MakeImpossible("DECIMAL", "IMAGE");
            MakeImpossible("DECIMAL", "TEXT");
            MakeImpossible("DECIMAL", "NTEXT");
            MakeImpossible("DECIMAL", "XML");
            MakeImpossible("DECIMAL", "HIERARCHYID");

            MakeImpossibleSameAs("NUMERIC", "DECIMAL");
            MakeImpossibleSameAs("FLOAT", "DECIMAL");
            MakeImpossibleSameAs("REAL", "DECIMAL");
            MakeImpossible("FLOAT", "TIMESTAMP");
            MakeImpossible("REAL", "TIMESTAMP");
            MakeImpossibleSameAs("BIGINT", "DECIMAL");
            MakeImpossibleSameAs("INT", "DECIMAL");
            MakeImpossibleSameAs("SMALLINT", "DECIMAL");
            MakeImpossibleSameAs("TINYINT", "DECIMAL");
            MakeImpossibleSameAs("BIT", "DECIMAL");

            MakeImpossibleSameAs("TIMESTAMP", "DECIMAL");
            MakeImpossible("TIMESTAMP", "NCHAR");
            MakeImpossible("TIMESTAMP", "NVARCHAR");
            MakeImpossible("TIMESTAMP", "FLOAT");
            MakeImpossible("TIMESTAMP", "REAL");
            // TODO : -> IMAGE is possible
            MakeImpossible("TIMESTAMP", "SQL_VARIANT");

            MakeImpossible("UNIQUEIDENTIFIER", "DATE");
            MakeImpossible("UNIQUEIDENTIFIER", "TIME");
            MakeImpossible("UNIQUEIDENTIFIER", "DATETIME");
            MakeImpossible("UNIQUEIDENTIFIER", "DATETIME2");
            MakeImpossible("UNIQUEIDENTIFIER", "SMALLDATTIME");
            MakeImpossible("UNIQUEIDENTIFIER", "DATETIMEOFFSET");
            MakeImpossible("UNIQUEIDENTIFIER", "TIMESTAMP");
            MakeImpossible("UNIQUEIDENTIFIER", "BIGINT");
            MakeImpossible("UNIQUEIDENTIFIER", "INT");
            MakeImpossible("UNIQUEIDENTIFIER", "SMALLINT");
            MakeImpossible("UNIQUEIDENTIFIER", "TINYINT");
            MakeImpossible("UNIQUEIDENTIFIER", "BIT");
            MakeImpossible("UNIQUEIDENTIFIER", "DECIMAL");
            MakeImpossible("UNIQUEIDENTIFIER", "NUMERIC");
            MakeImpossible("UNIQUEIDENTIFIER", "REAL");
            MakeImpossible("UNIQUEIDENTIFIER", "FLOAT");
            MakeImpossible("UNIQUEIDENTIFIER", "MONEY");
            MakeImpossible("UNIQUEIDENTIFIER", "SMALLMONEY");
            MakeImpossible("UNIQUEIDENTIFIER", "XML");
            MakeImpossible("UNIQUEIDENTIFIER", "IMAGE");
            MakeImpossible("UNIQUEIDENTIFIER", "TEXT");
            MakeImpossible("UNIQUEIDENTIFIER", "NTEXT");

            NoImplicitConversionTypes.Add("SQL_VARIANT");
            NoImplicitConversionTypes.Add("XML");
            NoImplicitConversionTypes.Add("CURSOR");
            NoImplicitConversionTypes.Add("HIERARCHYID");
            NoImplicitConversionTypes.Add("IMAGE"); // to binary it actually can
            NoImplicitConversionTypes.Add(TSqlDomainAttributes.DateTimePartEnum); // not a type but a set of magic words
        }

        public ImplicitConversionImpossibleRule() : base()
        {
        }

        public void SetParser(TSqlParser parser)
        {
            this.parser = parser;
        }

        public void LoadMetadata(SqlServerMetadata data)
        {
            metaData = data;
            knownReturnTypes = new Dictionary<string, string>(metaData.GlobalVariables, StringComparer.OrdinalIgnoreCase);

            foreach (var fn in metaData.Functions)
            {
                if (!string.IsNullOrEmpty(fn.Value.DataType))
                {
                    knownReturnTypes.Add(fn.Key, fn.Value.DataType);
                }
            }
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            Debug.Assert(metaData != null, "metaData not set");

            if (!ScalarExpressionEvaluator.IsBatchInteresting(node))
            {
                return;
            }

            var evaluator = new ExpressionResultTypeEvaluator(node)
                .InjectKnownReturnTypes(knownReturnTypes);

            var truncVisitor = new ImpossibleVisitor(
                Impossible,
                NoImplicitConversionTypes,
                evaluator,
                parser,
                ViolationHandlerWithMessage);

            node.AcceptChildren(truncVisitor);
        }

        private static void MakeImpossible(string typeA, string typeB)
        {
            if (!Impossible.TryGetValue(typeA, out var targetList))
            {
                targetList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Impossible.Add(typeA, targetList);
            }

            targetList.Add(typeB);
        }

        private static void MakeImpossibleSameAs(string typeA, string sameAsTypeB)
        {
            Impossible.Add(typeA, new HashSet<string>(Impossible[sameAsTypeB], StringComparer.OrdinalIgnoreCase));
        }
    }
}
