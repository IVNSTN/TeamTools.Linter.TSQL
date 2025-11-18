using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.Core
{
    public class SqlTypeResolver : ISqlTypeResolver
    {
        private readonly Dictionary<string, ISqlTypeHandler> knownTypes
            = new Dictionary<string, ISqlTypeHandler>(StringComparer.OrdinalIgnoreCase);

        public bool IsSupportedType(string typeName)
            => !string.IsNullOrEmpty(typeName) && knownTypes.ContainsKey(typeName);

        public ISqlTypeHandler ResolveTypeHandler(string typeName)
            => !string.IsNullOrEmpty(typeName) && knownTypes.TryGetValue(typeName, out var handler) ? handler : default;

        public void RegisterTypeHandler(ISqlTypeHandler typeHandler)
        {
            var types = typeHandler?.SupportedTypes ?? throw new ArgumentNullException(nameof(typeHandler));
            foreach (var tp in types)
            {
                // yes, it should fail in case of collision
                knownTypes.Add(tp, typeHandler);
            }
        }

        public SqlTypeReference ResolveType(DataTypeReference dataType)
        {
            if (dataType?.Name is null)
            {
                // e.g. CURSOR
                return default;
            }

            string typeName = dataType.GetFullName();

            // TODO : generate dummy-references for built-in
            // but currently unsupported types
            return ResolveTypeHandler(typeName)?
                .MakeSqlDataTypeReference(dataType);
        }

        // TODO : Type will have default size. This is not always OK.
        public SqlTypeReference ResolveType(string typeName)
            => ResolveTypeHandler(typeName)?.MakeSqlDataTypeReference(typeName);
    }
}
