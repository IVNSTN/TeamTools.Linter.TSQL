using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.XQueryFunctions
{
    public class XQueryValue : SqlGenericFunctionHandler<XQueryValue.XQueryValueArgs>
    {
        private static readonly string FuncName = "XQuery.Value";
        private static readonly int RequiredArgCount = 2;

        private static readonly Regex TypeNameWithParams = new Regex(
            @"(?<name>[a-zA-Z]+)[\s(]+(?<param1>\d+)[\s,)]*(?<param2>\d+)?[\s,)]*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        public XQueryValue() : base(FuncName, RequiredArgCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<XQueryValueArgs> call)
        {
            return ValidationScenario
                .For("XPATH_SELECTOR", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .Then(v => call.ValidatedArgs.Selector = v)
                && ValidationScenario
                .For("OUTPUT_TYPE", call.RawArgs[1], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.OutputType = s);
        }

        // Finally the XQuery Value() function call will be evaluated to an Unknown value of precise type.
        protected override string DoEvaluateResultType(CallSignature<XQueryValueArgs> call)
        {
            if (call.ValidatedArgs.OutputType?.IsPreciseValue != true
            || call.ValidatedArgs.OutputType.IsNull)
            {
                return default;
            }

            call.ValidatedArgs.OutputTypeRef = ResolveType(call.ValidatedArgs.OutputType.Value, call.Context.TypeResolver);
            return call.ValidatedArgs.OutputTypeRef?.TypeName;
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<XQueryValueArgs> call)
        {
            if (call.ValidatedArgs.OutputTypeRef != null)
            {
                // Unkwnown value with respect to datatype parameters
                return call.ResultTypeHandler.ValueFactory.NewValue(call.ValidatedArgs.OutputTypeRef, SqlValueKind.Unknown);
            }

            return base.DoEvaluateResultValue(call);
        }

        private static SqlTypeReference ResolveType(string typeDeclaration, ISqlTypeResolver typeResolver)
        {
            var typeRef = typeResolver.ResolveType(typeDeclaration);
            if (typeRef != null)
            {
                // Exact match of supported type without parameters, e.g. 'INT'
                return typeRef;
            }

            return ResolveTypeWithParams(typeDeclaration, typeResolver);
        }

        // Type with parameters, e.g. 'VARCHAR(100)', 'DECIMAL(20,10)'
        private static SqlTypeReference ResolveTypeWithParams(string typeDeclaration, ISqlTypeResolver typeResolver)
        {
            // TODO : Parsing by ScriptDom is preferred. But where to get parser instance from?
            var match = TypeNameWithParams.Match(typeDeclaration);
            if (match is null)
            {
                // something unsupported
                return default;
            }

            var typeDefinition = new SqlDataTypeReference
            {
                Name = new SchemaObjectName(),
            };

            typeDefinition.Name.Identifiers.Add(new Identifier { Value = match.Groups["name"].Value });
            typeDefinition.Parameters.Add(new IntegerLiteral { Value = match.Groups["param1"].Value });
            if (match.Groups["param2"] != null)
            {
                typeDefinition.Parameters.Add(new IntegerLiteral { Value = match.Groups["param2"].Value });
            }

            // Saving resolved type reference for later use in value evaluation.
            // Current method can return type name only.
            return typeResolver.ResolveType(typeDefinition);
        }

        [ExcludeFromCodeCoverage]
        public sealed class XQueryValueArgs
        {
            public SqlValue Selector { get; set; }

            public SqlStrTypeValue OutputType { get; set; }

            public SqlTypeReference OutputTypeRef { get; set; }
        }
    }
}
