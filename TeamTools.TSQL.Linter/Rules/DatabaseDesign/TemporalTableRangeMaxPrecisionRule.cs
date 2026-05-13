using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0866", "HISTORY_MAX_DATE_PRECISION")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed partial class TemporalTableRangeMaxPrecisionRule : BaseTemporalTableRangeValidator
    {
        public TemporalTableRangeMaxPrecisionRule() : base()
        {
        }

        // This rules ensures that max precision is defined only
        protected override bool DoesColumnHaveCorrectDefinition(ColumnDefinition col) => IsDataTypeAlright(col.DataType);
    }
}
