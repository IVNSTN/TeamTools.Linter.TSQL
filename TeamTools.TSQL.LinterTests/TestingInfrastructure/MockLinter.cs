using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TeamTools.TSQL.Linter.Infrastructure;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    public class MockLinter
    {
        private static readonly int DefaultCompatibilityLevel = 150;

        public MockLinter(int compatibilityLevel) : base()
        {
            Parser = new TSqlParserFactory().Make(compatibilityLevel);
            CompatibilityLevel = CompatibilityConverter.ToSqlVersion(compatibilityLevel);

            Meta = new SqlServerMetadata();
            InitMeta();
            InitEnumInfo();
        }

        public TSqlParser Parser { get; }

        public SqlServerMetadata Meta { get; }

        public SqlVersion CompatibilityLevel { get; }

        public static MockLinter MakeLinter()
        {
            if (!int.TryParse(TestContext.Parameters["CompatibilityLevel"], out int compat))
            {
                // Put hardcoded compatibility level temporarily here
                // to run tests for compatibility check during development in VS
                // or use specific runsettings file with CompatibilityLevel run parameter defined
                compat = DefaultCompatibilityLevel;
            }

            Debug.WriteLine($"Compatibility level = {compat}");

            return new MockLinter(compat);
        }

        public static MockLinter MakeDefaultLinter()
        {
            return new MockLinter(DefaultCompatibilityLevel);
        }

        public TSqlFragment Lint(string tsql, out IList<ParseError> err)
        {
            return Parser.Parse(new StringReader(tsql), out err);
        }

        public void Lint(string scriptPath, AbstractRule rule)
        {
            using var fileReader = new StreamReader(scriptPath);
            var parsedCode = Parser.Parse(fileReader, out IList<ParseError> parsingErrors);

            Assert.That(
                string.Join(
                    Environment.NewLine,
                    parsingErrors.Select(e => string.Format("({1},{2}) {0}", e.Message, e.Line, e.Column))),
                Is.Empty,
                "Failed parsing script");

            if (rule is IDynamicSqlParser dyn)
            {
                dyn.SetParser(Parser);
            }

            if (rule is IKeywordDetector kwd)
            {
                kwd.LoadKeywords(Meta.Keywords);
            }

            if (rule is ISqlServerMetadataConsumer sqlmeta)
            {
                sqlmeta.LoadMetadata(Meta);
            }

            if (rule is IFileLevelRule fileRule)
            {
                fileRule.VerifyFile(scriptPath, parsedCode);
            }
            else
            {
                parsedCode.Accept(rule);
            }
        }

        private void InitMeta()
        {
            // TODO : specific tests expect specific values - so move this initialization to specific test sets
            Meta.Keywords.Add("BEGIN");
            Meta.Keywords.Add("RETURN");
            Meta.Keywords.Add("ORDER");
            Meta.Keywords.Add("INT");
            Meta.Keywords.Add("THROW");

            Meta.GlobalVariables.Add("@@ROWCOUNT", "INT");
            Meta.GlobalVariables.Add("@@FETCH_STATUS", "INT");

            Meta.Functions.Add("GETDATE", new SqlMetaProgrammabilitySignature { DataType = "DATETIME", ParamCount = 0 });
            Meta.Functions.Add("SYSDATETIME", new SqlMetaProgrammabilitySignature { DataType = "DATETIME2", ParamCount = 0 });
            Meta.Functions.Add("DATENAME", new SqlMetaProgrammabilitySignature { DataType = "VARCHAR", ParamCount = 2 });
            Meta.Functions["DATENAME"].ParamDefinition.Add("0", TSqlDomainAttributes.DateTimePartEnum);
            Meta.Functions["DATENAME"].ParamDefinition.Add("1", "DATETIME");
            Meta.Functions.Add("DATEPART", new SqlMetaProgrammabilitySignature { DataType = "INT", ParamCount = 2 });
            Meta.Functions["DATEPART"].ParamDefinition.Add("0", TSqlDomainAttributes.DateTimePartEnum);
            Meta.Functions["DATEPART"].ParamDefinition.Add("1", "DATETIME");
            Meta.Functions.Add("DATEADD", new SqlMetaProgrammabilitySignature { DataType = "DATETIME", ParamCount = 3 });
            Meta.Functions["DATEADD"].ParamDefinition.Add("0", TSqlDomainAttributes.DateTimePartEnum);
            Meta.Functions["DATEADD"].ParamDefinition.Add("1", "DATETIME");
            Meta.Functions["DATEADD"].ParamDefinition.Add("2", "INT");
            Meta.Functions.Add("DATEDIFF", new SqlMetaProgrammabilitySignature { DataType = "INT", ParamCount = 3 });
            Meta.Functions["DATEDIFF"].ParamDefinition.Add("0", TSqlDomainAttributes.DateTimePartEnum);
            Meta.Functions["DATEDIFF"].ParamDefinition.Add("1", "DATETIME");
            Meta.Functions["DATEDIFF"].ParamDefinition.Add("2", "DATETIME");
            Meta.Functions.Add("DATEFROMPARTS", new SqlMetaProgrammabilitySignature { DataType = "DATE", ParamCount = 3 });

            Meta.Functions.Add("LOWER", new SqlMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 1 });
            Meta.Functions.Add("UPPER", new SqlMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 1 });
            Meta.Functions.Add("LTRIM", new SqlMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 1 });
            Meta.Functions.Add("RTRIM", new SqlMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 1 });
            Meta.Functions.Add("TRIM", new SqlMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCountMin = 1, ParamCountMax = 2 });
            Meta.Functions.Add("LEFT", new SqlMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 2 });
            Meta.Functions.Add("RIGHT", new SqlMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 2 });
            Meta.Functions.Add("REPLACE", new SqlMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 3 });

            Meta.Functions.Add("ISNULL", new SqlMetaProgrammabilitySignature { ParamCount = 2 });
            Meta.Functions.Add("NULLIF", new SqlMetaProgrammabilitySignature { ParamCount = 2 });
            Meta.Functions.Add("COALESCE", new SqlMetaProgrammabilitySignature { ParamCountMin = 2 });
            Meta.Functions.Add("IIF", new SqlMetaProgrammabilitySignature { ParamCount = 3 });
            Meta.Functions.Add("FORMATMESSAGE", new SqlMetaProgrammabilitySignature { DataType = "NVARCHAR", ParamCountMin = 2 });

            Meta.Functions.Add("YEAR", new SqlMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });
            Meta.Functions.Add("MONTH", new SqlMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });
            Meta.Functions.Add("DAY", new SqlMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });
            Meta.Functions.Add("SIGN", new SqlMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });
            Meta.Functions.Add("ISJSON", new SqlMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });

            Meta.Functions.Add("COUNT", new SqlMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });
            Meta.Functions.Add("MAX", new SqlMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("AVG", new SqlMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("SUM", new SqlMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("ROUND", new SqlMetaProgrammabilitySignature { ParamCountMin = 1, ParamCountMax = 2 });
            Meta.Functions.Add("CAST", new SqlMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("CONVERT", new SqlMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("TRY_CAST", new SqlMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("TRY_CONVERT", new SqlMetaProgrammabilitySignature { ParamCount = 1 });

            Meta.Functions.Add("NEWID", new SqlMetaProgrammabilitySignature { DataType = "UNIQUEIDENTIFIER", ParamCount = 0 });
            Meta.Functions.Add("EVENT_DATA", new SqlMetaProgrammabilitySignature { DataType = "XML", ParamCount = 0 });

            Meta.Enums.Add(TSqlDomainAttributes.DateTimePartEnum, new List<SqlServerMetadata.EnumElementProperties>());
        }

        private void InitEnumInfo()
        {
            var enumInfo = Meta.Enums[TSqlDomainAttributes.DateTimePartEnum];
            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "MS" });
            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "MILLISECOND" });
            enumInfo.Where(en => en.Name == "MS").First().Properties.Add("Alias", "MILLISECOND");
            enumInfo.Where(en => en.Name == "MILLISECOND").First().Properties.Add("Requires", "TIME");

            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "NS" });
            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "NANOSECOND" });
            enumInfo.Where(en => en.Name == "NS").First().Properties.Add("Alias", "NANOSECOND");
            enumInfo.Where(en => en.Name == "NANOSECOND").First().Properties.Add("Requires", "TIMENANO");

            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "S" });
            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "SECOND" });
            enumInfo.Where(en => en.Name == "S").First().Properties.Add("Alias", "SECOND");
            enumInfo.Where(en => en.Name == "SECOND").First().Properties.Add("Requires", "TIME");

            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "MI" });
            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "MINUTE" });
            enumInfo.Where(en => en.Name == "MI").First().Properties.Add("Alias", "MINUTE");
            enumInfo.Where(en => en.Name == "MINUTE").First().Properties.Add("Requires", "TIME");

            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "HH" });
            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "HOUR" });
            enumInfo.Where(en => en.Name == "HH").First().Properties.Add("Alias", "HOUR");
            enumInfo.Where(en => en.Name == "HOUR").First().Properties.Add("Requires", "TIME");

            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "YY" });
            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "YEAR" });
            enumInfo.Where(en => en.Name == "YY").First().Properties.Add("Alias", "YEAR");
            enumInfo.Where(en => en.Name == "YEAR").First().Properties.Add("Requires", "DATE");

            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "M" });
            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "MONTH" });
            enumInfo.Where(en => en.Name == "M").First().Properties.Add("Alias", "MONTH");
            enumInfo.Where(en => en.Name == "MONTH").First().Properties.Add("Requires", "DATE");

            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "DD" });
            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "DAY" });
            enumInfo.Where(en => en.Name == "DD").First().Properties.Add("Alias", "DAY");
            enumInfo.Where(en => en.Name == "DAY").First().Properties.Add("Requires", "DATE");

            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "Q" });
            enumInfo.Add(new SqlServerMetadata.EnumElementProperties { Name = "QUARTER" });
            enumInfo.Where(en => en.Name == "Q").First().Properties.Add("Alias", "QUARTER");
            enumInfo.Where(en => en.Name == "QUARTER").First().Properties.Add("Requires", "DATE");
        }
    }
}
