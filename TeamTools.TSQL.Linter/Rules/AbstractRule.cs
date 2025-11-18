using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Base rule class.
    /// </summary>
    public abstract partial class AbstractRule : ILinterRule
    {
        protected AbstractRule()
        {
        }

        public event EventHandler<RuleViolationEventDto> ViolationCallback;

        public void Subscribe(ViolationCallbackEvent callback)
        {
            if (callback != null)
            {
                ViolationCallback += (obj, dto) => callback(obj, dto);
            }
        }

        protected void ReportRuleViolation(int line, int pos, string details, TSqlFragment node = default, int? fragmentLength = null)
        {
            ViolationCallback?.Invoke(
                this,
                new RuleViolationEventDto
                {
                    ErrorDetails = details,
                    Line = line,
                    Column = pos,
                    FragmentLength = node?.FragmentLength ?? fragmentLength,
                });
        }
    }
}
