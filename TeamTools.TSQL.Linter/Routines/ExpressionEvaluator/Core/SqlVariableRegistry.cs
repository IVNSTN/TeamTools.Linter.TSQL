using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Core;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public class SqlVariableRegistry : IVariableRegistry, IVariableEvaluatedValueRegistry, IVariableEvaluator
    {
        private readonly IDictionary<string, SqlTypeReference> variables
            = new SortedDictionary<string, SqlTypeReference>(StringComparer.OrdinalIgnoreCase);

        private readonly IDictionary<string, IList<EvaluatedValueAtPos>> values
            = new SortedDictionary<string, IList<EvaluatedValueAtPos>>(StringComparer.OrdinalIgnoreCase);

        private readonly ISqlTypeConverter converter;
        private readonly IViolationRegistrar violations;

        public SqlVariableRegistry(ISqlTypeConverter converter, IViolationRegistrar violations)
        {
            this.converter = converter ?? throw new ArgumentNullException(nameof(converter));
            this.violations = violations ?? throw new ArgumentNullException(nameof(violations));
        }

        public IDictionary<string, SqlTypeReference> Variables => variables;

        public IDictionary<string, IList<EvaluatedValueAtPos>> Values => values;

        public SqlValue GetValueAt(string varName, int tokenIndex)
        {
            if (string.IsNullOrEmpty(varName) || tokenIndex < 0)
            {
                // invalid argument
                return default;
            }

            if (!variables.ContainsKey(varName))
            {
                // FIXME : register all vars first!
                // no matter if type is not supported by this evaluator engine
                // TODO : move away from this get-method
                // violations.RegisterViolation(new UnregisteredVariableViolation(varName, tokenIndex));
                return default;
            }

            if (!values.ContainsKey(varName))
            {
                // no values registered for this variable
                return default;
            }

            // TODO : optimize this search
            var value = values[varName]
                .Where(e => e.StartingFromTokenIndex <= tokenIndex)
                .OrderBy(e => e.StartingFromTokenIndex)
                .Select(e => e.EvaluatedValue)
                .LastOrDefault();

            if (value != null && value is IClonableValue c)
            {
                return c.Clone();
            }

            return value;
        }

        public void RegisterEvaluatedValue(string varName, int tokenIndex, SqlValue value)
        {
            if (value is null || tokenIndex < 0)
            {
                return;
            }

            if (!IsVariableRegistered(varName))
            {
                // FIXME : register all vars first!
                // no matter if type is not supported by this evaluator engine
                // violations.RegisterViolation(new UnregisteredVariableViolation(varName, tokenIndex));
                return;
            }

            if (!values.ContainsKey(varName))
            {
                values.Add(varName, new List<EvaluatedValueAtPos>());
            }

            // TODO : not sure about this Source replacement
            // specific value path should be build in a different way
            var origin = value.Source;
            if (value.Source != null && value.Source is SqlValueSourceVariable varSrc)
            {
                // if the value came from another variable then the origin is
                // that second variable value source
                origin = varSrc.Origin;
            }

            var typedValue = converter.ImplicitlyConvertTo(variables[varName], value);

            if (typedValue is null)
            {
                return;
            }

            if (typedValue != null && typedValue == value && typedValue is IClonableValue c)
            {
                typedValue = c.Clone();
            }

            typedValue.Source = new SqlValueSourceVariable(varName, origin, origin?.Node);
            values[varName].Add(new EvaluatedValueAtPos(tokenIndex, typedValue));
        }

        public void RegisterEvaluatedValue(string varName, int tokenIndex, SqlValueKind valueKind, SqlValueSource src)
        {
            var value = GetVariableTypeReference(varName)?.MakeValue(valueKind);

            if (value is null)
            {
                // TODO : or fail?
                return;
            }

            value.Source = src;

            RegisterEvaluatedValue(varName, tokenIndex, value);
        }

        // TODO : tbd
        public void Squash()
        {
            // throw new NotImplementedException();
        }

        public bool IsVariableRegistered(string varName)
            => !string.IsNullOrEmpty(varName) && variables.ContainsKey(varName);

        public void RegisterVariable(string varName, SqlTypeReference varType)
        {
            if (string.IsNullOrEmpty(varName))
            {
                throw new ArgumentNullException(nameof(varName));
            }

            if (varType is null)
            {
                throw new ArgumentNullException(nameof(varType));
            }

            if (IsVariableRegistered(varName))
            {
                // TODO : or error? or register a violation?
                return;
            }

            // TODO : store VarRef object with both varType and varName properties?
            variables.Add(varName, varType);
        }

        public SqlTypeReference GetVariableTypeReference(string varName)
            => IsVariableRegistered(varName) ? variables[varName] : default;

        public void ResetEvaluatedValuesAfterBlock(int fromTokenIndex, int tillTokenIndex, SqlValueSource src)
            => this.ResetValuesToUnknown(fromTokenIndex, tillTokenIndex, src);

        public void RevertValueEstimatesToBeforeBlock(int fromTokenIndex, int tillTokenIndex)
            => this.RevertValuesToPrior(fromTokenIndex, tillTokenIndex);
    }
}
