using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.Common.Linting;
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
        private static readonly string DefaultResultTableName = "RESULT";
        private static readonly string StringFallbackType = "VARCHAR";
        private static readonly IDictionary<string, ICollection<string>> Impossible = new SortedDictionary<string, ICollection<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly ICollection<string> NoImplicitConversionTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private IDictionary<string, string> knownReturnTypes;
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
            knownReturnTypes = metaData.Functions
                .Where(fn => !string.IsNullOrEmpty(fn.Value.DataType))
                .Select(fn => new KeyValuePair<string, string>(fn.Key, fn.Value.DataType))
                .Union(metaData.GlobalVariables)
                .ToDictionary(fn => fn.Key, fn => fn.Value, StringComparer.OrdinalIgnoreCase);
        }

        public override void Visit(TSqlBatch node)
        {
            Debug.Assert(metaData != null, "metaData not set");
            if (metaData == null)
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
                HandleNodeError);

            node.AcceptChildren(truncVisitor);
        }

        private static void MakeImpossible(string typeA, string typeB)
        {
            if (!Impossible.ContainsKey(typeA))
            {
                Impossible.Add(typeA, new SortedSet<string>(StringComparer.OrdinalIgnoreCase));
            }

            Impossible[typeA].Add(typeB);
        }

        private static void MakeImpossibleSameAs(string typeA, string sameAsTypeB)
        {
            Impossible.Add(typeA, new SortedSet<string>(StringComparer.OrdinalIgnoreCase));
            foreach (string typeB in Impossible[sameAsTypeB])
            {
                Impossible[typeA].Add(typeB);
            }
        }

        // high-level concept is very similar to ImplicitTruncationRule
        private class ImpossibleVisitor : TSqlFragmentVisitor
        {
            private static readonly string CallbackMsgTemplate = "{0} -> {1}";
            private readonly IDictionary<string, ICollection<string>> impossibleTypeConversions;
            private readonly ICollection<string> noImplicitConversionTypes;
            private readonly ExpressionResultTypeEvaluator typeEvaluator;
            private readonly TSqlParser parser;
            private readonly Action<TSqlFragment, string> callback;

            public ImpossibleVisitor(
                IDictionary<string, ICollection<string>> impossibleTypeConversions,
                ICollection<string> noImplicitConversionTypes,
                ExpressionResultTypeEvaluator typeEvaluator,
                TSqlParser parser,
                Action<TSqlFragment, string> callback)
            {
                this.impossibleTypeConversions = impossibleTypeConversions;
                this.noImplicitConversionTypes = noImplicitConversionTypes;
                this.typeEvaluator = typeEvaluator;
                this.parser = parser;
                this.callback = callback;
            }

            public override void Visit(DeclareTableVariableBody node)
            {
                var tableName = node.VariableName?.Value ?? DefaultResultTableName; // for inline table functions
                ValidateTableDefinition(tableName, node.Definition);
            }

            public override void Visit(CreateTableStatement node)
            {
                var tableName = node.SchemaObjectName.GetFullName();
                ValidateTableDefinition(tableName, node.Definition);
            }

            public override void Visit(BinaryQueryExpression node)
            {
                var specA = node.FirstQueryExpression.GetQuerySpecification();
                var specB = node.SecondQueryExpression.GetQuerySpecification(); // TODO : better support multiple nesting

                if (specA is null || specB is null)
                {
                    return;
                }

                ValidateSpecificationCompatibility(specA, specB);
            }

            public override void Visit(InsertSpecification node)
            {
                if (node.InsertSource is ExecuteInsertSource)
                {
                    return;
                }

                string targetName = default;
                if (node.Target is NamedTableReference tblName)
                {
                    targetName = tblName.SchemaObject.GetFullName();
                }
                else if (node.Target is VariableTableReference varName)
                {
                    targetName = varName.Variable.Name;
                }

                if (string.IsNullOrEmpty(targetName))
                {
                    return;
                }

                if ((node.Columns?.Count ?? 0) == 0)
                {
                    // TODO : remember order of columns in table definition and use it
                    return;
                }

                if (node.InsertSource is ValuesInsertSource val)
                {
                    if (val.RowValues.Count == 0)
                    {
                        return;
                    }

                    // required equality is handled by a separate rule
                    int n = val.RowValues[0].ColumnValues.Count > node.Columns.Count ? node.Columns.Count : val.RowValues[0].ColumnValues.Count;
                    for (int colIndex = 0; colIndex < n; colIndex++)
                    {
                        string columnType = typeEvaluator.GetColumnType(targetName, node.Columns[colIndex].MultiPartIdentifier.Identifiers.Last().Value);

                        if (string.IsNullOrEmpty(columnType))
                        {
                            continue;
                        }

                        for (int rowIndex = 0; rowIndex < val.RowValues.Count; rowIndex++)
                        {
                            ValidateCanConvertAtoB(val.RowValues[rowIndex].ColumnValues[colIndex], columnType);
                        }
                    }

                    return;
                }

                if (node.InsertSource is SelectInsertSource sel)
                {
                    var spec = sel.Select.GetQuerySpecification();
                    if (spec is null)
                    {
                        return;
                    }

                    // required equality is handled by a separate rule
                    int n = spec.SelectElements.Count > node.Columns.Count ? node.Columns.Count : spec.SelectElements.Count;
                    for (int colIndex = 0; colIndex < n; colIndex++)
                    {
                        string columnType = typeEvaluator.GetColumnType(targetName, node.Columns[colIndex].MultiPartIdentifier.Identifiers.Last().Value);

                        if (string.IsNullOrEmpty(columnType))
                        {
                            continue;
                        }

                        if (!(spec.SelectElements[colIndex] is SelectScalarExpression selectScalar))
                        {
                            continue;
                        }

                        ValidateCanConvertAtoB(selectScalar.Expression, columnType);
                    }

                    return;
                }
            }

            // Variable default value must be compatible with variable type
            public override void Visit(DeclareVariableElement node)
            {
                if (node.Value is null)
                {
                    return;
                }

                ValidateCanConvertAtoB(node.Value, typeEvaluator.GetExpressionType(node));
            }

            // All parts of scalar computations must be compatible
            public override void Visit(BinaryExpression node)
            {
                var resultingType = typeEvaluator.GetExpressionType(node.FirstExpression, node.SecondExpression);
                if (string.IsNullOrEmpty(resultingType))
                {
                    return;
                }

                ValidateCanConvertAtoB(node.FirstExpression, typeEvaluator.GetExpressionType(node.FirstExpression), resultingType);
                ValidateCanConvertAtoB(node.SecondExpression, typeEvaluator.GetExpressionType(node.SecondExpression), resultingType);
            }

            // Left and right sides of comparison must be compatible
            public override void Visit(BooleanComparisonExpression node)
            {
                var resultingType = typeEvaluator.GetExpressionType(node.FirstExpression, node.SecondExpression);
                if (string.IsNullOrEmpty(resultingType))
                {
                    return;
                }

                ValidateCanConvertAtoB(node.FirstExpression, typeEvaluator.GetExpressionType(node.FirstExpression), resultingType);
                ValidateCanConvertAtoB(node.SecondExpression, typeEvaluator.GetExpressionType(node.SecondExpression), resultingType);
            }

            // BETWEEN is a syntax sugar for >= AND <= thus checked value must be compatible with both range limits
            public override void Visit(BooleanTernaryExpression node)
            {
                ValidateCanConvertAtoB(node.FirstExpression, node.SecondExpression);
                ValidateCanConvertAtoB(node.FirstExpression, node.ThirdExpression);
            }

            // Checking if we can assign expression result type to variable of it's own type
            public override void Visit(SetVariableStatement node)
            {
                var variableType = typeEvaluator.GetExpressionType(node.Variable);
                if (string.IsNullOrEmpty(variableType))
                {
                    return;
                }

                if (node.Expression is null)
                {
                    ValidateCanConvertAtoB(node.Variable, variableType);
                }
                else
                {
                    ValidateCanConvertAtoB(node.Expression, variableType);
                }
            }

            // Checking if we can assign expression result type to variable of it's own type
            public override void Visit(SelectSetVariable node)
            {
                var variableType = typeEvaluator.GetExpressionType(node.Variable);
                if (string.IsNullOrEmpty(variableType))
                {
                    return;
                }

                ValidateCanConvertAtoB(node.Expression, variableType);
            }

            // NULLIF result type is the type of the first argument
            public override void Visit(NullIfExpression node)
            {
                ValidateCanConvertAtoB(node.SecondExpression, node.FirstExpression);
            }

            // Don't check ConvertCall, TryConvertCall and so on here!
            // Explicit conversions have more options.
            // Current rule is for IMPLICIT conversions only.
            public override void Visit(TryParseCall node)
            {
                ValidateCanConvertAtoB(node.StringValue, StringFallbackType);
            }

            public override void Visit(ParseCall node)
            {
                ValidateCanConvertAtoB(node.StringValue, StringFallbackType);
            }

            // COALESCE result type is the type with highest precision out of all the input expressions
            public override void Visit(CoalesceExpression node)
            {
                var resultingType = typeEvaluator.GetExpressionType(node.Expressions);
                if (string.IsNullOrEmpty(resultingType))
                {
                    return;
                }

                foreach (var e in node.Expressions)
                {
                    ValidateCanConvertAtoB(e, typeEvaluator.GetExpressionType(e), resultingType);
                }
            }

            // IIF result type is the type with highest precision from THEN and ELSE expressions
            public override void Visit(IIfCall node)
            {
                var resultingType = typeEvaluator.GetExpressionType(node.ThenExpression, node.ElseExpression);
                if (string.IsNullOrEmpty(resultingType))
                {
                    return;
                }

                ValidateCanConvertAtoB(node.ThenExpression, typeEvaluator.GetExpressionType(node.ThenExpression), resultingType);
                ValidateCanConvertAtoB(node.ElseExpression, typeEvaluator.GetExpressionType(node.ElseExpression), resultingType);
            }

            // ISNULL result type is the type of the first argument
            public override void Visit(FunctionCall node)
            {
                if (!node.FunctionName.Value.Equals("ISNULL", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (node.Parameters.Count < 2)
                {
                    return;
                }

                ValidateCanConvertAtoB(node.Parameters[1], node.Parameters[0]);
            }

            // EXECUTE return value type is INT
            public override void Visit(ExecuteSpecification node)
            {
                if (node.Variable is null)
                {
                    return;
                }

                ValidateCanConvertAtoB("INT", node.Variable);
            }

            // sp_executesql parameters defined must be compatible with passed params
            // implementation is very similar to SpExecuteParamDefinitionMatchRule
            public override void Visit(ExecutableProcedureReference node)
            {
                var declaredParameters = SpExecuteParameterExtractor.ExtractDeclaredParameters(parser, node);
                if (declaredParameters is null || !declaredParameters.Any())
                {
                    return;
                }

                // first two params are scipt itself and input param definition
                var passedArguments = node.Parameters.Skip(2).ToList();
                bool namedCall = passedArguments.Any(p => p.Variable != null);
                // same number of declared and passed params is checked by separate rule
                int n = declaredParameters.Count > passedArguments.Count && !namedCall ? passedArguments.Count : declaredParameters.Count;

                for (int paramIndex = 0; paramIndex < n; paramIndex++)
                {
                    string paramType = typeEvaluator.GetExpressionType(declaredParameters[paramIndex]);
                    if (string.IsNullOrEmpty(paramType))
                    {
                        continue;
                    }

                    ExecuteParameter arg;
                    if (namedCall)
                    {
                        string paramName = declaredParameters[paramIndex].VariableName.Value;
                        arg = passedArguments
                            .FirstOrDefault(p => p.Variable?.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase) ?? false);
                    }
                    else
                    {
                        arg = passedArguments[paramIndex];
                    }

                    if (arg is null || arg.ParameterValue is null)
                    {
                        continue;
                    }

                    ValidateCanConvertAtoB(arg.ParameterValue, paramType);
                }
            }

            // CASE result type is the type with highest precision out of all THEN/ELSE expressions
            public override void Visit(SearchedCaseExpression node)
            {
                var thenExpressions = node.WhenClauses.Select(w => w.ThenExpression);
                var resultingType = typeEvaluator.GetExpressionType(thenExpressions);
                if (string.IsNullOrEmpty(resultingType))
                {
                    return;
                }

                // all THEN results must be compatible with CASE output type
                foreach (var then in thenExpressions)
                {
                    ValidateCanConvertAtoB(then, resultingType);
                }

                if (node.ElseExpression != null)
                {
                    ValidateCanConvertAtoB(node.ElseExpression, resultingType);
                }
            }

            // CASE result type is the type with highest precision out of all THEN/ELSE expressions
            // also in CASE @val WHEN ... all WHEN expressions must be compatible with input expression
            public override void Visit(SimpleCaseExpression node)
            {
                var inputType = typeEvaluator.GetExpressionType(node.InputExpression);
                // all WHEN values are compatible when CASE input expression
                if (!string.IsNullOrEmpty(inputType))
                {
                    foreach (var when in node.WhenClauses)
                    {
                        ValidateCanConvertAtoB(when.WhenExpression, inputType);
                    }
                }

                var thenExpressions = node.WhenClauses.Select(w => w.ThenExpression);
                var resultingType = typeEvaluator.GetExpressionType(thenExpressions);
                if (string.IsNullOrEmpty(resultingType))
                {
                    return;
                }

                // all THEN results are compatible with CASE output type
                foreach (var then in thenExpressions)
                {
                    ValidateCanConvertAtoB(then, resultingType);
                }

                if (node.ElseExpression != null)
                {
                    ValidateCanConvertAtoB(node.ElseExpression, resultingType);
                }
            }

            public override void Visit(InPredicate node)
            {
                string resultingType = node.Values != null
                    ? typeEvaluator.GetExpressionType(node.Values)
                    : typeEvaluator.GetExpressionType(node.Subquery);

                // checked expression must be compatible with values resulting type
                ValidateCanConvertAtoB(node.Expression, resultingType);

                // and each value must be compatible with this type
                foreach (var v in node.Values)
                {
                    ValidateCanConvertAtoB(v, resultingType);
                }
            }

            private void ValidateSpecificationCompatibility(QuerySpecification specA, QuerySpecification specB)
            {
                // required equality is handled by a separate rule
                int n = specA.SelectElements.Count > specB.SelectElements.Count ? specB.SelectElements.Count : specA.SelectElements.Count;

                for (int i = 0; i < n; i++)
                {
                    // TODO : grab source table name to reach to cached column types
                    if (!(specA.SelectElements[i] is SelectScalarExpression exprA))
                    {
                        continue;
                    }

                    if (!(specB.SelectElements[i] is SelectScalarExpression exprB))
                    {
                        continue;
                    }

                    ValidateCanConvertAtoB(exprB.Expression, exprA.Expression);
                }
            }

            private void ValidateTableDefinition(string tableName, TableDefinition node)
            {
                if (node is null)
                {
                    // e.g. filetable
                    return;
                }

                // caching column types for use in constraint and insert validation
                foreach (var col in node.ColumnDefinitions)
                {
                    string colType = typeEvaluator.GetColumnType(tableName, col);

                    if (col.DefaultConstraint != null)
                    {
                        ValidateCanConvertAtoB(col.DefaultConstraint.Expression, colType);
                    }

                    var check = col.Constraints.OfType<CheckConstraintDefinition>().FirstOrDefault();
                    if (check != null)
                    {
                        // TODO:
                        // Validate check.CheckCondition;
                    }
                }

                // checks and default defined on table level
                foreach (var cstr in node.TableConstraints)
                {
                    if (cstr is CheckConstraintDefinition check)
                    {
                        // Column check constraint expressions must be compatible with column type
                        // TODO:
                        // Validate check.CheckCondition;
                    }
                    else if (cstr is DefaultConstraintDefinition def)
                    {
                        // Column default value must be compatible with column type
                        string colType = typeEvaluator.GetColumnType(tableName, def.Column.Value);
                        ValidateCanConvertAtoB(def.Expression, colType);
                    }
                }
            }

            private void ValidateCanConvertAtoB(ScalarExpression expressionA, ScalarExpression expressionB)
            {
                ValidateCanConvertAtoB(expressionA, typeEvaluator.GetExpressionType(expressionA), typeEvaluator.GetExpressionType(expressionB));
            }

            private void ValidateCanConvertAtoB(ScalarExpression expression, string typeName)
            {
                ValidateCanConvertAtoB(expression, typeEvaluator.GetExpressionType(expression), typeName);
            }

            private void ValidateCanConvertAtoB(string typeName, ScalarExpression expression)
            {
                ValidateCanConvertAtoB(expression, typeName, typeEvaluator.GetExpressionType(expression));
            }

            private void ValidateCanConvertAtoB(TSqlFragment node, string typeA, string typeB)
            {
                if (string.IsNullOrEmpty(typeA) || string.IsNullOrEmpty(typeB))
                {
                    return;
                }

                if (string.Equals(typeA, typeB, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (noImplicitConversionTypes.Contains(typeA) || typeB == "CURSOR")
                {
                    callback(node, string.Format(CallbackMsgTemplate, typeA, typeB));
                    return;
                }

                if (!impossibleTypeConversions.ContainsKey(typeA))
                {
                    return;
                }

                if (!impossibleTypeConversions[typeA].Contains(typeB))
                {
                    return;
                }

                callback(node, string.Format(CallbackMsgTemplate, typeA, typeB));
            }
        }
    }
}
