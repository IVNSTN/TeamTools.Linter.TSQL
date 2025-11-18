using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator
{
    // TODO : Combine with ExpressionEvaluator solution
    public sealed class ExpressionResultTypeEvaluator
    {
        private static readonly string StringFallbackType = "NVARCHAR";
        private static readonly List<string> TypePrecedence;
        private static readonly Dictionary<LiteralType, string> LiteralTypeMap;
        private static readonly HashSet<string> StringOrBinaryCompatibleTypes;
        private static readonly Dictionary<string, string> TypeAliases;

        // variable names with their types
        private Dictionary<string, string> declarations;
        // cached computed expression types
        private Dictionary<TSqlFragment, string> expressionTypes;

        private Dictionary<string, string> functionResultTypes;

        static ExpressionResultTypeEvaluator()
        {
            // TODO : consolidate all info in SqlServerMetadata file
            // keep in this order!
            TypePrecedence = new List<string>
            {
                "SQL_VARIANT",
                "XML",
                "DATETIMEOFFSET",
                "DATETIME2",
                "DATETIME",
                "SMALLDATETIME",
                "DATE",
                "TIME",
                "FLOAT",
                "REAL",
                "DECIMAL",
                "MONEY",
                "SMALLMONEY",
                "BIGINT",
                "INT",
                "SMALLINT",
                "TINYINT",
                "BIT",
                "NTEXT",
                "TEXT",
                "IMAGE",
                "TIMESTAMP",
                "UNIQUEIDENTIFIER",
                "NVARCHAR",
                "SYSNAME",
                "NCHAR",
                "VARCHAR",
                "CHAR",
                "VARBINARY",
                "BINARY",
            };

            LiteralTypeMap = new Dictionary<LiteralType, string>
            {
                { LiteralType.Integer, "INT" },
                { LiteralType.Binary, "BINARY" },
                { LiteralType.Money, "MONEY" },
                { LiteralType.Numeric, "DECIMAL" },
                { LiteralType.Real, "REAL" },
                { LiteralType.String, "VARCHAR" },
            };

            StringOrBinaryCompatibleTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "VARCHAR",
                "CHAR",
                "NVARCHAR",
                "SYSNAME",
                "NCHAR",
                "VARBINARY",
                "BINARY",
                "IMAGE",
            };

            TypeAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "SYSNAME", "NVARCHAR" },
            };

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
            scriptWithDeclarations.Accept(new VariableDeclarationVisitor(d => _ = GetExpressionType(d)));
        }

        public static string GetResultingType(IEnumerable<string> typeList)
        {
            if (typeList is null)
            {
                return default;
            }

            var types = typeList.ToArray();
            if (types.Length == 0)
            {
                return default;
            }

            if (types.Length == 1)
            {
                return types[0];
            }

            var typeA = types[0];
            foreach (var typeB in types)
            {
                if (string.IsNullOrEmpty(typeB))
                {
                    // unpredictable outcome
                    return default;
                }

                if (!typeB.Equals(typeA, StringComparison.OrdinalIgnoreCase))
                {
                    typeA = GetResultingType(typeA, typeB);
                }
            }

            if (!string.IsNullOrEmpty(typeA) && TypeAliases.TryGetValue(typeA, out var typeAlias))
            {
                return typeAlias;
            }

            return typeA;
        }

        public ExpressionResultTypeEvaluator InjectKnownReturnTypes(Dictionary<string, string> knownTypes)
        {
            functionResultTypes = knownTypes;

            return this;
        }

        public string GetColumnType(string tableName, string columnName)
        {
            if (declarations is null || declarations.Count == 0)
            {
                return default;
            }

            string columnId = $"{tableName}{TSqlDomainAttributes.NamePartSeparator}{columnName}";
            if (declarations.TryGetValue(columnId, out string colType))
            {
                return colType;
            }

            return default;
        }

        public string GetColumnType(string tableName, ColumnDefinition node)
        {
            string columnId = $"{tableName}{TSqlDomainAttributes.NamePartSeparator}{node.ColumnIdentifier.Value}";

            if (declarations != null && declarations.TryGetValue(columnId, out string colType))
            {
                return colType;
            }

            var typeName = node.ComputedColumnExpression != null
                ? GetExpressionType(node.ComputedColumnExpression)
                : GetResultingType(node.DataType);
            if (string.IsNullOrEmpty(typeName))
            {
                return default;
            }

            if (declarations is null)
            {
                declarations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            declarations.Add(columnId, typeName);

            return typeName;
        }

        public string GetExpressionType(DeclareVariableElement node)
        {
            string variableName = node.VariableName.Value;
            Debug.Assert(!string.IsNullOrEmpty(variableName), "variable name not defined");

            if (declarations != null && declarations.TryGetValue(variableName, out string varType))
            {
                return varType;
            }

            var typeName = GetResultingType(node.DataType);
            if (string.IsNullOrEmpty(typeName))
            {
                return default;
            }

            if (declarations is null)
            {
                declarations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            declarations.Add(variableName, typeName);

            return typeName;
        }

        public string GetExpressionType(IList<ScalarExpression> expr)
            => GetResultingType(GetExpressionTypes(expr));

        public string GetExpressionType(IEnumerable<ScalarExpression> expr)
            => GetResultingType(GetExpressionTypes(expr));

        public string GetExpressionType(params ScalarExpression[] expr)
        {
            if (expr is null || expr.Length == 0)
            {
                return default;
            }

            return GetResultingType(GetExpressionTypes(expr));
        }

        public IEnumerable<string> GetExpressionTypes(IList<ScalarExpression> expr)
        {
            int n = expr.Count;
            for (int i = 0; i < n; i++)
            {
                var e = expr[i];
                if (e != null)
                {
                    yield return GetExpressionType(e);
                }
            }
        }

        public IEnumerable<string> GetExpressionTypes(IEnumerable<ScalarExpression> expr)
        {
            foreach (var e in expr)
            {
                yield return GetExpressionType(e);
            }
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

            return typeA.Name.BaseIdentifier.Value;
        }

        private string ComputeExpressionType(ScalarExpression expr)
        {
            Debug.Assert(expr != null, "expression is null");

            string exprType = default;

            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expressionTypes?.TryGetValue(expr, out exprType) ?? false)
            {
                return exprType;
            }

            if (expr is BinaryExpression bin)
            {
                exprType = GetResultingType(ComputeExpressionType(bin.FirstExpression), ComputeExpressionType(bin.SecondExpression));
            }
            else if (expr is UnaryExpression ue)
            {
                exprType = ComputeExpressionType(ue.Expression);
            }
            else if (expr is VariableReference vref)
            {
                Debug.Assert(!string.IsNullOrEmpty(vref.Name), "var ref name null");

                if (declarations != null && declarations.TryGetValue(vref.Name, out string varType))
                {
                    exprType = varType;
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

            if (string.IsNullOrEmpty(exprType))
            {
                return exprType;
            }

            Debug.Assert(expr != null, "expression node null");
            if (expressionTypes is null)
            {
                expressionTypes = new Dictionary<TSqlFragment, string>();
            }

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
                    if (forXml.Options.HasOption(XmlForClauseOptions.Type))
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
                LiteralTypeMap.TryGetValue(literal.LiteralType, out var literalType);
                return literalType;
            }

            if (node is FunctionCall fn)
            {
                string functionName = fn.FunctionName.Value;
                Debug.Assert(!string.IsNullOrEmpty(functionName), "function name empty");

                if (functionResultTypes?.TryGetValue(functionName, out var resultType) ?? false)
                {
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

                if (functionResultTypes != null && functionResultTypes.TryGetValue(globalVarName, out string globalVarType))
                {
                    return globalVarType;
                }
                else
                {
                    return default;
                }
            }

            if (node is SimpleCaseExpression simpleCase)
            {
                return GetExpressionType(simpleCase.ExtractOutputExpressions());
            }

            if (node is SearchedCaseExpression searchCase)
            {
                return GetExpressionType(searchCase.ExtractOutputExpressions());
            }

            return default;
        }

        private sealed class VariableDeclarationVisitor : TSqlFragmentVisitor
        {
            private readonly Action<DeclareVariableElement> callback;

            public VariableDeclarationVisitor(Action<DeclareVariableElement> callback)
            {
                this.callback = callback;
            }

            public override void Visit(DeclareVariableElement node) => callback(node);
        }
    }
}
