using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Registers return-value pseudo variable for further return value type validation.
    /// </summary>
    public partial class VariableDeclarationVisitor
    {
        private void RegisterReturnValue(StatementList body, string typeName)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                // no body - no return
                return;
            }

            DoRegisterReturnValueOfType(typeResolver.ResolveType(typeName));
        }

        private void RegisterReturnValue(StatementList body, DataTypeReference typeDefinition)
        {
            DoRegisterReturnValueOfType(typeResolver.ResolveType(typeDefinition));
        }

        private void DoRegisterReturnValueOfType(SqlTypeReference typeRef)
        {
            if (typeRef is null)
            {
                // unsupported type
                return;
            }

            const string returnValueVarPseudoName = "RETURN_VALUE";
            varRegistry.RegisterVariable(returnValueVarPseudoName, typeRef);
        }
    }
}
