using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
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
            var declaredParameters = SpExecuteParameterExtractor.ExtractDeclaredParameters(parser, node);
            if (declaredParameters is null || !declaredParameters.Any())
            {
                return;
            }

            // first two params are scipt itself and input param definition
            var passedParameters = node.Parameters.Skip(2).ToList();

            ValidateParamDefinition(declaredParameters, passedParameters);
        }

        private void ValidateParamDefinition(IList<ProcedureParameter> declarations, IList<ExecuteParameter> parameters)
        {
            if (declarations.Count != parameters.Count)
            {
                HandleNodeError(parameters[0], "param count");
                return;
            }

            foreach (ExecuteParameter p in parameters.Where(p => p.Variable != null))
            {
                var found = declarations.FirstOrDefault(d =>
                    string.Equals(d.VariableName.Value, p.Variable.Name, StringComparison.OrdinalIgnoreCase));

                if (found is null || found.IsOutput() != p.IsOutput)
                {
                    HandleNodeError(p);
                }
            }
        }
    }
}
