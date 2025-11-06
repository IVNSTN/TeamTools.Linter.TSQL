using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    public interface IDeprecationHandler
    {
        void LoadDeprecations(IDictionary<string, string> values);
    }

    public interface IKeywordDetector
    {
        void LoadKeywords(ICollection<string> values);
    }

    public interface ICommentAnalyzer
    {
        void LoadSpecialCommentPrefixes(ICollection<string> values);
    }

    public interface IDynamicSqlParser
    {
        void SetParser(TSqlParser parser);
    }

    public interface ISqlServerMetadataConsumer
    {
        void LoadMetadata(SqlServerMetadata data);
    }
}
