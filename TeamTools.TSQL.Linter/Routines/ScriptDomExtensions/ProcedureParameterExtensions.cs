using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class ProcedureParameterExtensions
    {
        public static bool IsOutput(this ProcedureParameter param)
            => (param.Modifier & ParameterModifier.Output) == ParameterModifier.Output;

        public static bool IsReadOnly(this ProcedureParameter param)
            => (param.Modifier & ParameterModifier.ReadOnly) == ParameterModifier.ReadOnly;
    }
}
