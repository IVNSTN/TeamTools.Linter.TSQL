using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;

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
        private static readonly HashSet<string> ExpectedIsolationLevels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "REPEATABLEREAD",
            "SERIALIZABLE",
            "SNAPSHOT",
        };

        private readonly NativelyUnsupportedInstructionDetector instructionDetector;

        public NativelyUnsupportedInstructionRule() : base()
        {
            instructionDetector = new NativelyUnsupportedInstructionDetector(ViolationHandlerWithMessage);
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DoValidate(proc);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                DoValidate(trg);
            }
            else if (firstStmt is FunctionStatementBody fn)
            {
                DoValidate(fn);
            }
        }

        private static void ExtractNativeCompilationOptions(IList<AtomicBlockOption> options, out IdentifierAtomicBlockOption isolation, out AtomicBlockOption language)
        {
            isolation = default;
            language = default;

            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                var opt = options[i];
                if (opt is IdentifierAtomicBlockOption atom
                && atom.OptionKind == AtomicBlockOptionKind.IsolationLevel)
                {
                    isolation = atom;
                }
                else if (opt.OptionKind == AtomicBlockOptionKind.Language)
                {
                    language = opt;
                }
            }
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
                ExtractNativeCompilationOptions(ba.Options, out var isolation, out var language);

                if (isolation is null)
                {
                    HandleNodeError(ba, Strings.ViolationDetails_NativelyUnsupportedInstructionRule_IsolationLevelMissing);
                }
                else if (!ExpectedIsolationLevels.Contains(isolation.Value.Value))
                {
                    HandleNodeError(isolation, string.Format(Strings.ViolationDetails_NativelyUnsupportedInstructionRule_BadIsolationLevel, isolation.Value.Value));
                }

                if (language is null)
                {
                    HandleNodeError(ba, Strings.ViolationDetails_NativelyUnsupportedInstructionRule_LanguageMissing);
                }
            }
            else if (needsAtomicBlock)
            {
                HandleNodeError(body.Statements[0], Strings.ViolationDetails_NativelyUnsupportedInstructionRule_AtomicBeginMissing);
            }

            body.Accept(instructionDetector);
        }
    }
}
