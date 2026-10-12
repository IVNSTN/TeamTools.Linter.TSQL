using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class DropWithNoCheckRule
    {
        [ExcludeFromCodeCoverage]
        private static class ObjectTypeHandler
        {
            private static readonly Dictionary<string, string> ObjectTypeAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Different types of functions all dropped via DROP FUNCTION
                { "IF", "FN" },
                { "FT", "FN" },
                { "FS", "FN" },
                { "AF", "FN" },

                // Extended and CLR stored proc - dropped via DROP PROC
                { "PC", "P" },
                { "X", "P" },

                // Different types of constraints - dropped via ALTER TABLE DROP CONSTRAINT
                { "D", "C" },
                { "F", "C" },
                { "PK", "C" },
                { "UQ", "C" },

                // CLR trigger
                { "TA", "TR" },
            };

            public static string GetObjectTypeName(TSqlFragment node)
            {
                if (node is DropDefaultStatement
                || node is CreateDefaultStatement)
                {
                    // "D" normalized to "C" - _a_ constraint
                    // as defined in type alias dictionary above
                    return "C";
                }

                if (node is DropFunctionStatement
                || node is CreateFunctionStatement)
                {
                    return "FN";
                }

                if (node is DropProcedureStatement
                || node is CreateProcedureStatement)
                {
                    return "P";
                }

                if (node is DropRuleStatement
                || node is CreateRuleStatement)
                {
                    return "R";
                }

                if (node is DropSequenceStatement
                || node is CreateSequenceStatement)
                {
                    return "SQ";
                }

                if (node is DropSynonymStatement
                || node is CreateSynonymStatement)
                {
                    return "SN";
                }

                if (node is DropTableStatement
                || node is CreateTableStatement)
                {
                    return "U";
                }

                if (node is DropTriggerStatement
                || node is CreateTriggerStatement)
                {
                    return "TR";
                }

                if (node is DropViewStatement
                || node is CreateViewStatement)
                {
                    return "V";
                }

                if (node is DropTypeStatement
                || node is CreateTypeStatement)
                {
                    return "TT";
                }

                if (node is DropSchemaStatement
                || node is CreateSchemaStatement)
                {
                    return "SCM";
                }

                // unsupported object type
                return default;
            }

            public static string GetObjectTypeName(string objectType)
            {
                if (ObjectTypeAliases.TryGetValue(objectType, out var typeAlias))
                {
                    // object type normalization: e.g. all types of functions
                    // are dropped via DROP FUNCTION statement
                    return typeAlias;
                }

                return objectType;
            }

            public static bool IsSupportedTableElementType(TableElementType tableElementType)
            {
                switch (tableElementType)
                {
                    case TableElementType.Constraint:
                        return true;
                    default:
                        return false;
                }
            }

            public static string GetObjectTypeName(TableElementType tableElementType)
            {
                switch (tableElementType)
                {
                    // OBJECT_ID can not check columns or indexes
                    case TableElementType.Constraint:
                        return "C";
                    default:
                        // unsupported table element type
                        return default;
                }
            }
        }
    }
}
