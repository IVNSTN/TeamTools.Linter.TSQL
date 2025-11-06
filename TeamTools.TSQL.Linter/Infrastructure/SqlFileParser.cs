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
            int n = errors.Count;

            if (n > 0)
            {
                var err = new List<Exception>();

                for (int i = 0; i < n; i++)
                {
                    err.Add(new ParsingException(errors[i].Message, errors[i].Line, errors[i].Column));
                }

                throw new AggregateException(err);
            }

            return sqlFragment;
        }
    }
}
