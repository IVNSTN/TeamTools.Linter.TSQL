using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class ObjectOptionsExtensions
    {
        public static bool HasOption(this IList<ProcedureOption> options, ProcedureOptionKind subj)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i].OptionKind == subj)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasOption(this IList<FunctionOption> options, FunctionOptionKind subj)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i].OptionKind == subj)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasOption(this IList<TriggerOption> options, TriggerOptionKind subj)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i].OptionKind == subj)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasOption(this IList<TableOption> options, TableOptionKind subj)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i].OptionKind == subj)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasOption(this IList<IndexOption> options, IndexOptionKind subj)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i].OptionKind == subj)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasOption(this IList<XmlForClauseOption> options, XmlForClauseOptions subj)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i].OptionKind == subj)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasOption(this IList<QueueOption> options, QueueOptionKind subj)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i].OptionKind == subj)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasOption(this IList<CursorOption> options, CursorOptionKind subj)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i].OptionKind == subj)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasHint(this IList<OptimizerHint> hints, OptimizerHintKind subj)
        {
            int n = hints.Count;
            for (int i = 0; i < n; i++)
            {
                if (hints[i].HintKind == subj)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
