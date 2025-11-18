using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    internal class SqlFileParser : IFileParser<TSqlFragment>
    {
        private readonly TSqlParser parser;

        public SqlFileParser(TSqlParser parser)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public TSqlFragment Parse(ILintingContext context)
        {
            TSqlFragment sqlFragment = parser.Parse(context.FileContents, out IList<ParseError> errors);

            if (errors.Count > 0)
            {
                ReThrowParserErrors(errors);
            }

            return sqlFragment;
        }

        private static void ReThrowParserErrors(IList<ParseError> errors)
        {
            int n = errors.Count;
            var err = new List<Exception>(n);

            for (int i = 0; i < n; i++)
            {
                var e = errors[i];
                err.Add(new ParsingException(e.Message, e.Line, e.Column));
            }

            throw new AggregateException(err);
        }
    }
}
