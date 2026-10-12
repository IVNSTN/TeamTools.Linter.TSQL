using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class DropWithNoCheckRule
    {
        private sealed class ConditionalDropVisitor : TSqlFragmentVisitor
        {
            private const string ObjectIdFunctionName = "OBJECT_ID";
            private const string SchemaIdFunctionName = "SCHEMA_ID";

            private readonly List<TSqlFragment> suspiciousDrops;

            public ConditionalDropVisitor(List<TSqlFragment> suspiciousDrops)
            {
                this.suspiciousDrops = suspiciousDrops;
            }

            public override void Visit(IfStatement node)
            {
                var existenceChecks = new HashSet<string>(ExtractObjectExistenceChecks(node.Predicate, false), StringComparer.OrdinalIgnoreCase);

                if (existenceChecks.Count == 0)
                {
                    return;
                }

                var nestedDropsVisitor = new NestedDropsVisitor(suspiciousDrops, existenceChecks);
                node.ThenStatement.Accept(nestedDropsVisitor);
            }

            // IF multiple object existence checks are combined with AND operator
            // it will return all checked object names
            private static IEnumerable<string> ExtractObjectExistenceChecks(BooleanExpression node, bool negated)
            {
                while (node is BooleanParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is BooleanNotExpression ne)
                {
                    return ExtractObjectExistenceChecks(ne.Expression, !negated);
                }

                if (node is BooleanBinaryExpression bin)
                {
                    if ((negated && bin.BinaryExpressionType == BooleanBinaryExpressionType.Or)
                    || ((!negated) && bin.BinaryExpressionType == BooleanBinaryExpressionType.And))
                    {
                        // OR is an unusual and hardly predictable logic for object existence check
                        // thus ignoring such cases
                        return ExtractObjectExistenceChecks(bin.FirstExpression, negated)
                            .Union(ExtractObjectExistenceChecks(bin.SecondExpression, negated));
                    }
                    else
                    {
                        return Enumerable.Empty<string>();
                    }
                }

                if (node is BooleanIsNullExpression isnull
                && (isnull.IsNot ^ negated))
                {
                    return Enumerable.Repeat<string>(ExtractObjectExistenceCheck(isnull.Expression), 1);
                }

                return Enumerable.Empty<string>();
            }

            private static string ExtractObjectExistenceCheck(ScalarExpression node)
            {
                while (node is ParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (!(node is FunctionCall fn
                && fn.Parameters.Count > 0
                && fn.Parameters[0].ExtractScalarExpression() is StringLiteral objectName))
                {
                    // something other than OBJECT_ID or SCHEMA_ID call
                    return default;
                }

                string fullName = objectName.Value;

                if (fn.FunctionName.Value.Equals(SchemaIdFunctionName, StringComparison.OrdinalIgnoreCase))
                {
                    // schema existence checked
                    return $"SCM:{fullName}";
                }

                if (!fn.FunctionName.Value.Equals(ObjectIdFunctionName, StringComparison.OrdinalIgnoreCase))
                {
                    // not an object existence check
                    return default;
                }

                fullName = NormalizeObjectName(fullName);

                if (fn.Parameters.Count == 2
                && fn.Parameters[1].ExtractScalarExpression() is StringLiteral objectType)
                {
                    string validatedObjectType = ObjectTypeHandler.GetObjectTypeName(objectType.Value);

                    // if object type provided then dropping object of this type only is valid
                    fullName = $"{validatedObjectType}:{fullName}";
                }

                return fullName;
            }

            private static string NormalizeObjectName(string originalName)
            {
                if (!originalName.StartsWith(TSqlDomainAttributes.TempTablePrefix)
                && !originalName.StartsWith(TSqlDomainAttributes.VariablePrefix)
                && !originalName.Contains(TSqlDomainAttributes.NamePartSeparator))
                {
                    // no schema prefix and this is not @ or # - prepending with "dbo."
                    return TSqlDomainAttributes.DefaultSchemaPrefix + originalName;
                }

                // temp objects need tempdb prefix in object existence check but
                // such prefix is never used in a DROP statement
                const string tempdbPrefix = "tempdb..#";
                const int tempdbObjectNameStart = 8;
                if (originalName.StartsWith(tempdbPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return originalName.Substring(tempdbObjectNameStart);
                }

                const string tempdbDboPrefix = "tempdb.dbo.#";
                const int tempdbDboObjectNameStart = 11;
                if (originalName.StartsWith(tempdbDboPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return originalName.Substring(tempdbDboObjectNameStart);
                }

                return originalName;
            }
        }
    }
}
