using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Natively unsupported instructions detection.
    /// </summary>
    [RuleIdentity("FA0761", "NATIVELY_UNSUPPORTED_INSTRUCTION")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [InMemoryRule]
    internal sealed partial class NativelyUnsupportedInstructionRule : AbstractRule
    {
        private static readonly ICollection<string> ExpectedIsolationLevels = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SNAPSHOT",
            "REPEATABLEREAD",
            "SERIALIZABLE",
        };

        public NativelyUnsupportedInstructionRule() : base()
        {
        }

        private void DoValidateStatements(TSqlFragment parent, StatementList body, bool needsAtomicBlock = true)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                if (needsAtomicBlock)
                {
                    HandleNodeError(parent, "EXTERNAL");
                }

                return;
            }

            if (body.Statements[0] is BeginEndAtomicBlockStatement ba)
            {
                var isolation = ba.Options
                    .OfType<IdentifierAtomicBlockOption>()
                    .FirstOrDefault(opt => opt.OptionKind == AtomicBlockOptionKind.IsolationLevel)
                    ?.Value;

                if (isolation is null)
                {
                    HandleNodeError(ba, "ISOLATION LEVEL option required");
                }
                else if (!ExpectedIsolationLevels.Contains(isolation.Value))
                {
                    HandleNodeError(isolation, $"unsupported isolation level {isolation.Value}");
                }

                if (!ba.Options.Any(opt => opt.OptionKind == AtomicBlockOptionKind.Language))
                {
                    HandleNodeError(ba, "LANGUAGE option required");
                }
            }
            else if (needsAtomicBlock)
            {
                HandleNodeError(body.Statements[0], "ATOMIC block expected");
            }

            body.Accept(new NativelyUnsupportedInstructionDetector(HandleNodeError));
        }
    }
}
