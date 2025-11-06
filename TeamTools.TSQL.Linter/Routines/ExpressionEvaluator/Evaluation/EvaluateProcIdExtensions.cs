using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    public static class EvaluateProcIdExtensions
    {
        private static readonly string ProcIdVar = "@@PROCID";

        public static void EvaluateProcId(
            this SqlScriptAnalyzer scriptAnalyzer,
            SchemaObjectName name,
            TSqlFragment node,
            int firstTokenIndex,
            int lastTokenIndex)
        {
            string schemaName = name.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;
            string triggerName = name.BaseIdentifier.Value;

            // TODO : simplify
            var intHandler = scriptAnalyzer.Converter.ImplicitlyConvert<SqlIntTypeValue>(
                scriptAnalyzer.TypeResolver.ResolveTypeHandler("dbo.INT").ValueFactory.NewNull(node)).TypeHandler;

            scriptAnalyzer.RegisterScopeLimitedProcIdValue(
                intHandler,
                schemaName,
                triggerName,
                firstTokenIndex,
                lastTokenIndex,
                node);
        }

        public static void RegisterScopeLimitedProcIdValue(
            this SqlScriptAnalyzer scriptAnalyzer,
            SqlIntTypeHandler typeHandler,
            string schemaName,
            string objectName,
            int firstToken,
            int lastToken,
            TSqlFragment node)
        {
            var procInfo = new CurrentProcReference(
                schemaName,
                objectName,
                typeHandler,
                new SqlValueSource(SqlValueSourceKind.Expression, node));

            // While we are inside a proc, we know exactly what SCHEMA_NAME(@@PROCID)
            // and other @@PROCID-related functions result is.
            scriptAnalyzer.VarRegistry.RegisterEvaluatedValue(ProcIdVar, firstToken, procInfo);
            scriptAnalyzer.VarRegistry.RegisterEvaluatedValue(ProcIdVar, lastToken, SqlValueKind.Unknown, null);
        }
    }
}
