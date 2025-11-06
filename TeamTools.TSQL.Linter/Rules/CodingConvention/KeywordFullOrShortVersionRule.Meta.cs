using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Metadata loader and dictionary initializer.
    /// </summary>
    internal partial class KeywordFullOrShortVersionRule : ISqlServerMetadataConsumer
    {
        // key - keyword, value - bad token, correct spelling
        private static readonly Lazy<IDictionary<KeywordWithShorthand, KeyValuePair<TSqlTokenType, string>>> KeywordSpellingInstance
            = new Lazy<IDictionary<KeywordWithShorthand, KeyValuePair<TSqlTokenType, string>>>(() => InitKeywordSpellingInstance(), true);

        private readonly IDictionary<string, string> dateParts = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly ICollection<string> dateFunctions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        private enum KeywordWithShorthand
        {
            Exec,
            Proc,
            Tran,
            Output,
            DatePart,
        }

        private static IDictionary<KeywordWithShorthand, KeyValuePair<TSqlTokenType, string>> KeywordSpelling => KeywordSpellingInstance.Value;

        public void LoadMetadata(SqlServerMetadata data)
        {
            dateParts.Clear();
            if (data.Enums.ContainsKey(TSqlDomainAttributes.DateTimePartEnum))
            {
                foreach (var datePart in data.Enums[TSqlDomainAttributes.DateTimePartEnum])
                {
                    if (datePart.Properties.TryGetValue("Alias", out string alias))
                    {
                        dateParts.Add(datePart.Name, alias);
                    }
                }
            }

            dateFunctions.Clear();
            var dateFunctionsInfo = data.Functions.Where(f => f.Value.ParamDefinition != null
                && f.Value.ParamDefinition.Any(p => p.Value.Equals(TSqlDomainAttributes.DateTimePartEnum, StringComparison.OrdinalIgnoreCase)));
            foreach (var fn in dateFunctionsInfo)
            {
                dateFunctions.Add(fn.Key);
            }
        }

        private static IDictionary<KeywordWithShorthand, KeyValuePair<TSqlTokenType, string>> InitKeywordSpellingInstance()
        {
            return new SortedDictionary<KeywordWithShorthand, KeyValuePair<TSqlTokenType, string>>
            {
                { KeywordWithShorthand.Exec, MakeKeywordMeta(TSqlTokenType.Execute, "EXEC") },
                { KeywordWithShorthand.Tran, MakeKeywordMeta(TSqlTokenType.Tran, "TRANSACTION") },
                { KeywordWithShorthand.Proc, MakeKeywordMeta(TSqlTokenType.Proc, "PROCEDURE") },
                { KeywordWithShorthand.Output, MakeKeywordMeta(default, "OUTPUT") }, // sorry for some magic
            };
        }

        private static KeyValuePair<TSqlTokenType, string> MakeKeywordMeta(TSqlTokenType token, string spelling)
        {
            return new KeyValuePair<TSqlTokenType, string>(token, spelling);
        }
    }
}
