using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public class SqlFunctionRegistry : ISqlFunctionRegistry
    {
        private readonly IDictionary<string, SqlFunctionHandler> functions
            = new SortedDictionary<string, SqlFunctionHandler>(StringComparer.OrdinalIgnoreCase);

        public SqlFunctionRegistry()
        {
        }

        public IDictionary<string, SqlFunctionHandler> Functions => functions;

        public bool IsFunctionRegistered(string name)
            => !string.IsNullOrEmpty(name) && functions.ContainsKey(name);

        public void RegisterFunction(SqlFunctionHandler def)
        {
            if (def is null)
            {
                throw new ArgumentNullException(nameof(def));
            }

            if (IsFunctionRegistered(def.FunctionName))
            {
                throw new ArgumentException($"Function '{def.FunctionName}' is already registered", nameof(def));
            }

            functions.Add(def.FunctionName, def);
        }

        public SqlFunctionHandler GetFunction(string name)
            => IsFunctionRegistered(name) ? functions[name] : default;
    }
}
