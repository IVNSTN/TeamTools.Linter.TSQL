using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0178", "SP_EXECUTESQL_PARAMS_MISMATCH")]
    internal sealed class SpExecuteParamDefinitionMatchRule : AbstractRule, IDynamicSqlParser
    {
        private TSqlParser parser = null;

        public SpExecuteParamDefinitionMatchRule() : base()
        {
        }

        public void SetParser(TSqlParser parser)
        {
            this.parser = parser;
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.Parameters.Count < 2)
            {
                // just a script to execute, no args
                return;
            }

            var declaredParameters = SpExecuteParameterExtractor.ExtractDeclaredParameters(parser, node);
            if (declaredParameters is null || declaredParameters.Count == 0)
            {
                return;
            }

            // first two params are scipt itself and input param definition
            ValidateParamDefinition(declaredParameters, node.Parameters, 2);
        }

        private void ValidateParamDefinition(IList<ProcedureParameter> declarations, IList<ExecuteParameter> parameters, int paramStart)
        {
            int passedParamCount = parameters.Count - paramStart;
            if (declarations.Count != passedParamCount)
            {
                HandleNodeError(parameters[0], string.Format(Strings.ViolationDetails_SpExecuteParamDefinitionMatchRule_ParamCountMismatch, declarations.Count.ToString(), passedParamCount.ToString()));
                return;
            }

            var declaredParams = declarations
                .ToDictionary(
                    d => d.VariableName.Value,
                    d => d.Modifier == ParameterModifier.Output,
                    StringComparer.OrdinalIgnoreCase);

            for (int i = paramStart, n = parameters.Count; i < n; i++)
            {
                var p = parameters[i];
                if (p.Variable is null)
                {
                    continue;
                }

                if (!declaredParams.TryGetValue(p.Variable.Name, out bool wasDeclaredAsOutput)
                || p.IsOutput != wasDeclaredAsOutput)
                {
                    // No such parameter declared or OUTPUT flag differs
                    HandleNodeError(p);
                }
            }
        }
    }
}
