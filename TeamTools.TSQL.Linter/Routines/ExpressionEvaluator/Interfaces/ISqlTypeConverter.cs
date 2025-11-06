using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface ISqlTypeConverter
    {
        // FIXME : this is bullshit
        T ImplicitlyConvert<T>(SqlValue from)
        where T : SqlValue;

        SqlValue ImplicitlyConvertTo(string typeName, SqlValue from);

        SqlValue ImplicitlyConvertTo(SqlTypeReference typeReference, SqlValue from);

        SqlValue ExplicitlyConvertTo(SqlTypeReference typeReference, SqlValue from, Action<string> callback = null);

        string EvaluateOutputType(params SqlValue[] values);

        string EvaluateOutputType(IList<SqlValue> values);
    }
}
