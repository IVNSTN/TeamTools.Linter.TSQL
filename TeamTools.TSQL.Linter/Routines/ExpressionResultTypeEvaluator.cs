using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    // TODO : Combine with ExpressionEvaluator solution
    internal sealed class ExpressionResultTypeEvaluator
    {
        private static readonly string StringFallbackType = "NVARCHAR";
        private static readonly List<string> TypePrecedence = new List<string>();
        private static readonly IDictionary<LiteralType, string> LiteralTypeMap = new Dictionary<LiteralType, string>();
        private static readonly ICollection<string> StringOrBinaryCompatibleTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly IDictionary<string, string> TypeAliases = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, string> functionResultTypes = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // variable names with their types
        private readonly IDictionary<string, string> declarations = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // cached computed expression types
        private readonly IDictionary<TSqlFragment, string> expressionTypes = new Dictionary<TSqlFragment, string>();

        static ExpressionResultTypeEvaluator()
        {
            // keep in this order!
            TypePrecedence.Add("SQL_VARIANT");
            TypePrecedence.Add("XML");
            TypePrecedence.Add("DATETIMEOFFSET");
            TypePrecedence.Add("DATETIME2");
            TypePrecedence.Add("DATETIME");
            TypePrecedence.Add("SMALLDATETIME");
            TypePrecedence.Add("DATE");
            TypePrecedence.Add("TIME");
            TypePrecedence.Add("FLOAT");
            TypePrecedence.Add("REAL");
            TypePrecedence.Add("DECIMAL");
            TypePrecedence.Add("MONEY");
            TypePrecedence.Add("SMALLMONEY");
            TypePrecedence.Add("BIGINT");
            TypePrecedence.Add("INT");
            TypePrecedence.Add("SMALLINT");
            TypePrecedence.Add("TINYINT");
            TypePrecedence.Add("BIT");
            TypePrecedence.Add("NTEXT");
            TypePrecedence.Add("TEXT");
            TypePrecedence.Add("IMAGE");
            TypePrecedence.Add("TIMESTAMP");
            TypePrecedence.Add("UNIQUEIDENTIFIER");
            TypePrecedence.Add("NVARCHAR");
            TypePrecedence.Add("SYSNAME");
            TypePrecedence.Add("NCHAR");
            TypePrecedence.Add("VARCHAR");
            TypePrecedence.Add("CHAR");
            TypePrecedence.Add("VARBINARY");
            TypePrecedence.Add("BINARY");

            LiteralTypeMap.Add(LiteralType.Integer, "INT");
            LiteralTypeMap.Add(LiteralType.Binary, "BINARY");
            LiteralTypeMap.Add(LiteralType.Money, "MONEY");
            LiteralTypeMap.Add(LiteralType.Numeric, "DECIMAL");
            LiteralTypeMap.Add(LiteralType.Real, "REAL");
            LiteralTypeMap.Add(LiteralType.String, "VARCHAR");

            StringOrBinaryCompatibleTypes.Add("VARCHAR");
            StringOrBinaryCompatibleTypes.Add("CHAR");
            StringOrBinaryCompatibleTypes.Add("NVARCHAR");
            StringOrBinaryCompatibleTypes.Add("SYSNAME");
            StringOrBinaryCompatibleTypes.Add("NCHAR");
            StringOrBinaryCompatibleTypes.Add("VARBINARY");
            StringOrBinaryCompatibleTypes.Add("BINARY");
            StringOrBinaryCompatibleTypes.Add("IMAGE");

            TypeAliases.Add("SYSNAME", "NVARCHAR");

            // TODO : update resource file
            // DATALENGTH, CHARINDEX, PATINDEX will return BIGINT for MAX size
            // SUBSTRING also can accept TEXT, NTEXT, IMAGE

            // TODO : in resource file split Scalar, Analytical, Aggregate functions
            // functions based on resultsets cannot show up in scalar expression
            // e.g. as a default value in DECLARE or table column definition

            // TODO : is operator precedence related to expressoin type evaluation? Feels like it is.
            // keep in this order!
            // 1 UnaryExpressionType
            // 2 BinaryExpressionType.Multiply, BinaryExpressionType.Divide, BinaryExpressionType.Modulo
            // 3 other BinaryExpressionType values
            // 4 all BooleanComparisonType
            // 5 NOT
            // 6 BooleanBinaryExpressionType.And
            // 7 BooleanBinaryExpressionType.Or, BETWEEN
            // 8 assignment
        }

        public ExpressionResultTypeEvaluator(TSqlFragment scriptWithDeclarations)
        {
            if (scriptWithDeclarations is null)
            {
                throw new ArgumentNullException(nameof(scriptWithDeclarations));
            }

            // extracting all variable declarations and caching their types
            scriptWithDeclarations.Accept(new VariableDeclarationVisitor(decl => GetExpressionType(decl)));
        }

        public static string GetResultingType(IEnumerable<string> types)
        {
            if (types is null || !types.Any() || types.Any(typeName => string.IsNullOrEmpty(typeName)))
            {
                return default;
            }

            var typeA = types.First();
            foreach (var typeB in types)
            {
                if (!typeB.Equals(typeA, StringComparison.OrdinalIgnoreCase))
                {
                    typeA = GetResultingType(typeA, typeB);
                }
            }

            if (!string.IsNullOrEmpty(typeA) && TypeAliases.ContainsKey(typeA))
            {
                typeA = TypeAliases[typeA];
            }

            return typeA;
        }

        public ExpressionResultTypeEvaluator InjectKnownReturnTypes(IDictionary<string, string> knownTypes)
        {
            foreach (var known in knownTypes)
            {
                functionResultTypes.Add(known.Key, known.Value);
            }

            return this;
        }

        public string GetColumnType(string tableName, string columnName)
        {
            string columnId = string.Join(TSqlDomainAttributes.NamePartSeparator, tableName, columnName);
            if (declarations.ContainsKey(columnId))
            {
                return declarations[columnId];
            }

            return default;
        }

        public string GetColumnType(string tableName, ColumnDefinition node)
        {
            string columnId = string.Join(TSqlDomainAttributes.NamePartSeparator, tableName, node.ColumnIdentifier.Value);

            if (!declarations.ContainsKey(columnId))
            {
                var typeName = node.ComputedColumnExpression != null
                    ? GetExpressionType(node.ComputedColumnExpression)
                    : GetResultingType(node.DataType);
                if (string.IsNullOrEmpty(typeName))
                {
                    return default;
                }

                declarations.Add(columnId, typeName);
            }

            return declarations[columnId];
        }

        public string GetExpressionType(DeclareVariableElement node)
        {
            string variableName = node.VariableName.Value;
            Debug.Assert(!string.IsNullOrEmpty(variableName), "variable name not defined");

            if (!declarations.ContainsKey(variableName))
            {
                var typeName = GetResultingType(node.DataType);
                if (string.IsNullOrEmpty(typeName))
                {
                    return default;
                }

                declarations.Add(variableName, typeName);
            }

            return declarations[variableName];
        }

        public string GetExpressionType(IEnumerable<ScalarExpression> expr)
            => GetResultingType(GetExpressionTypes(expr));

        public string GetExpressionType(params ScalarExpression[] expr)
        {
            if (expr is null || !expr.Any())
            {
                return default;
            }

            return GetResultingType(GetExpressionTypes(expr.ToList()));
        }

        public IEnumerable<string> GetExpressionTypes(IEnumerable<ScalarExpression> expr)
        {
            foreach (var e in expr.Where(e => e != null))
            {
                yield return GetExpressionType(e);
            }

            yield break;
        }

        public string GetExpressionType(ScalarExpression expr)
        {
            if (expr is null)
            {
                return default;
            }

            return ComputeExpressionType(expr);
        }

        private static string GetResultingType(string typeA, string typeB)
        {
            int typePrecedenceA = TypePrecedence.IndexOf(typeA);
            int typePrecedenceB = TypePrecedence.IndexOf(typeB);

            if (typePrecedenceB < 0 || typePrecedenceA < 0)
            {
                // unknown types
                return default;
            }

            // not checking type compatibility, just computing precedence
            // the outer code is supposed to check
            // it may request output type of the whole expression (one with highest precedence)
            // and then individually chek if each and every type can be converted to it
            if (typePrecedenceA < typePrecedenceB)
            {
                // higher precision has smaller index in list
                return typeA;
            }

            return typeB;
        }

        private static string GetResultingType(DataTypeReference typeA)
        {
            if (typeA is null)
            {
                return default;
            }

            if (typeA.Name?.SchemaIdentifier != null)
            {
                // no info about what system type is behind this UDT
                return default;
            }

            if (typeA.Name is null)
            {
                if (typeA is SqlDataTypeReference st)
                {
                    if (st.SqlDataTypeOption == SqlDataTypeOption.Cursor)
                    {
                        return "CURSOR";
                    }

                    Debug.Assert(false, "unsupported type");
                    return default;
                }

                Debug.Assert(false, "unsupported type");
                return default;
            }

            return typeA.Name.BaseIdentifier.Value.ToUpper();
        }

        private string ComputeExpressionType(ScalarExpression expr)
        {
            Debug.Assert(expr != null, "expression is null");

            if (expressionTypes.ContainsKey(expr))
            {
                return expressionTypes[expr];
            }

            string exprType = default;

            if (expr is BinaryExpression bin)
            {
                exprType = GetResultingType(ComputeExpressionType(bin.FirstExpression), ComputeExpressionType(bin.SecondExpression));
            }
            else if (expr is ParenthesisExpression pe)
            {
                exprType = ComputeExpressionType(pe.Expression);
            }
            else if (expr is UnaryExpression ue)
            {
                exprType = ComputeExpressionType(ue.Expression);
            }
            else if (expr is VariableReference vref)
            {
                Debug.Assert(!string.IsNullOrEmpty(vref.Name), "var ref name null");

                if (declarations.ContainsKey(vref.Name))
                {
                    exprType = declarations[vref.Name];
                }
            }
            else if (expr is ScalarSubquery q)
            {
                return GetSelectedExpressionType(q.QueryExpression);
            }
            else
            {
                exprType = ExtractTypeFromSimpleExpression(expr);
            }

            Debug.Assert(expr != null, "expression node null");
            expressionTypes.Add(expr, exprType);

            return exprType;
        }

        private string GetSelectedExpressionType(QueryExpression node)
        {
            var spec = node.GetQuerySpecification();

            if (spec is null)
            {
                return default;
            }

            if (spec.ForClause != null)
            {
                if (spec.ForClause is XmlForClause forXml)
                {
                    if (forXml.Options.Any(opt => opt.OptionKind == XmlForClauseOptions.Type))
                    {
                        return "XML";
                    }

                    // FOR XML without TYPE can be assigned to any string variable
                    // also NVARCHAR can be implicitly converted to XML, thus choosing NVARCHAR over XML
                    return StringFallbackType;
                }

                if (spec.ForClause is JsonForClause)
                {
                    return StringFallbackType;
                }

                // unsupported FOR clause
                return default;
            }

            if (spec.SelectElements.Count == 1 && spec.SelectElements[0] is SelectScalarExpression scalar)
            {
                return ComputeExpressionType(scalar.Expression);
            }

            return default;
        }

        private string ExtractTypeFromSimpleExpression(ScalarExpression node)
        {
            if (node is Literal literal)
            {
                if (LiteralTypeMap.ContainsKey(literal.LiteralType))
                {
                    return LiteralTypeMap[literal.LiteralType];
                }

                return default;
            }

            if (node is FunctionCall fn)
            {
                string functionName = fn.FunctionName.Value;
                Debug.Assert(!string.IsNullOrEmpty(functionName), "function name empty");

                if (functionResultTypes.ContainsKey(functionName))
                {
                    string resultType = functionResultTypes[functionName];
                    // some functions are applicable to VARCHAR/VARBINARY types and return the original type result
                    if (resultType == "STR/BIN")
                    {
                        if (fn.Parameters?.Count > 0)
                        {
                            resultType = GetExpressionType(fn.Parameters[0]);

                            if (!StringOrBinaryCompatibleTypes.Contains(resultType))
                            {
                                // unsupported case
                                return default;
                            }

                            return resultType;
                        }

                        // TODO : or unknown?
                        return StringFallbackType;
                    }

                    return resultType;
                }

                if (functionName.Equals("ISNULL", StringComparison.OrdinalIgnoreCase) && fn.Parameters?.Count > 0)
                {
                    return GetExpressionType(fn.Parameters[0]);
                }
            }

            // function calls below are not descendants of FunctionCall
            // thus handling them explicitly
            if (node is CastCall cast)
            {
                return GetResultingType(cast.DataType);
            }

            if (node is ConvertCall convert)
            {
                return GetResultingType(convert.DataType);
            }

            if (node is TryCastCall trycast)
            {
                return GetResultingType(trycast.DataType);
            }

            if (node is ParseCall parse)
            {
                return GetResultingType(parse.DataType);
            }

            if (node is TryParseCall tryparse)
            {
                return GetResultingType(tryparse.DataType);
            }

            if (node is TryConvertCall tryconvert)
            {
                return GetResultingType(tryconvert.DataType);
            }

            if (node is IIfCall iif)
            {
                return GetExpressionType(iif.ThenExpression, iif.ElseExpression);
            }

            if (node is CoalesceExpression coalesce)
            {
                return GetExpressionType(coalesce.Expressions);
            }

            if (node is NullIfExpression nullif)
            {
                return GetExpressionType(nullif.FirstExpression);
            }

            if (node is GlobalVariableExpression glob)
            {
                string globalVarName = glob.Name;
                Debug.Assert(!string.IsNullOrEmpty(globalVarName), "global var name empty");

                if (functionResultTypes.ContainsKey(globalVarName))
                {
                    return functionResultTypes[globalVarName];
                }
            }

            if (node is SimpleCaseExpression simpleCase)
            {
                return GetExpressionType(
                    simpleCase.WhenClauses
                        .Select(when => when.ThenExpression)
                        .Union(Enumerable.Repeat(simpleCase.ElseExpression, 1)));
            }

            if (node is SearchedCaseExpression searchCase)
            {
                return GetExpressionType(
                    searchCase.WhenClauses
                        .Select(when => when.ThenExpression)
                        .Union(Enumerable.Repeat(searchCase.ElseExpression, 1)));
            }

            return default;
        }

        private class VariableDeclarationVisitor : TSqlFragmentVisitor
        {
            private readonly Action<DeclareVariableElement> callback;

            public VariableDeclarationVisitor(Action<DeclareVariableElement> callback)
            {
                this.callback = callback;
            }

            public override void Visit(DeclareVariableElement node)
            {
                callback(node);
            }
        }
    }
}
