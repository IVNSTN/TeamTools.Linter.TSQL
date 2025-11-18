using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.Linter;
using TeamTools.TSQL.Linter.Infrastructure;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    public class MockLinter
    {
        private static readonly int DefaultCompatibilityLevel = 150;
        private readonly ScriptAnalysisServiceProvider svcProvider;

        public MockLinter(int compatibilityLevel) : base()
        {
            Parser = TSqlParserFactory.Make(compatibilityLevel);
            CompatibilityLevel = CompatibilityConverter.ToSqlVersion(compatibilityLevel);

            Meta = new SqlServerMetadata();
            InitMeta();
            InitEnumInfo();
            InitTypeInfo();

            svcProvider = new ScriptAnalysisServiceProvider();

            // TODO : refactoring needed. current implementation is just a proof of concept.
            svcProvider.RegisterServiceFactory((TSqlScript script) =>
            {
                var svc = new MainScriptObjectDetector();
                svc.Analyze(script);
                return svc;
            });

            svcProvider.RegisterServiceFactory((TSqlScript script) => new TableDefinitionElementsEnumerator(script));

            svcProvider.RegisterServiceFactory((TSqlBatch batch) => new ScalarExpressionEvaluator(batch));

            svcProvider.RegisterServiceFactory((TSqlScript script) =>
            {
                var svc = new TableIndicesVisitor();
                script.Accept(svc);
                return svc;
            });

            svcProvider.RegisterServiceFactory((TSqlBatch batch) =>
            {
                var svc = new ParenthesisParser(batch);
                svc.Parse();
                return svc;
            });
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

            if (rule is IScriptAnalysisServiceConsumer consumer)
            {
                consumer.InjectServiceProvider(svcProvider);
            }

            if (rule is IFileLevelRule fileRule)
            {
                fileRule.VerifyFile(scriptPath, parsedCode);
            }
            else
            {
                rule.Validate(parsedCode);
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

            Meta.Functions.Add("GETDATE", new SqlServerMetaProgrammabilitySignature { DataType = "DATETIME", ParamCount = 0 });
            Meta.Functions.Add("SYSDATETIME", new SqlServerMetaProgrammabilitySignature { DataType = "DATETIME2", ParamCount = 0 });
            Meta.Functions.Add("DATENAME", new SqlServerMetaProgrammabilitySignature { DataType = "VARCHAR", ParamCount = 2 });
            Meta.Functions["DATENAME"].ParamDefinition.Add("0", TSqlDomainAttributes.DateTimePartEnum);
            Meta.Functions["DATENAME"].ParamDefinition.Add("1", "DATETIME");
            Meta.Functions.Add("DATEPART", new SqlServerMetaProgrammabilitySignature { DataType = "INT", ParamCount = 2 });
            Meta.Functions["DATEPART"].ParamDefinition.Add("0", TSqlDomainAttributes.DateTimePartEnum);
            Meta.Functions["DATEPART"].ParamDefinition.Add("1", "DATETIME");
            Meta.Functions.Add("DATEADD", new SqlServerMetaProgrammabilitySignature { DataType = "DATETIME", ParamCount = 3 });
            Meta.Functions["DATEADD"].ParamDefinition.Add("0", TSqlDomainAttributes.DateTimePartEnum);
            Meta.Functions["DATEADD"].ParamDefinition.Add("1", "DATETIME");
            Meta.Functions["DATEADD"].ParamDefinition.Add("2", "INT");
            Meta.Functions.Add("DATEDIFF", new SqlServerMetaProgrammabilitySignature { DataType = "INT", ParamCount = 3 });
            Meta.Functions["DATEDIFF"].ParamDefinition.Add("0", TSqlDomainAttributes.DateTimePartEnum);
            Meta.Functions["DATEDIFF"].ParamDefinition.Add("1", "DATETIME");
            Meta.Functions["DATEDIFF"].ParamDefinition.Add("2", "DATETIME");
            Meta.Functions.Add("DATEFROMPARTS", new SqlServerMetaProgrammabilitySignature { DataType = "DATE", ParamCount = 3 });

            Meta.Functions.Add("LOWER", new SqlServerMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 1 });
            Meta.Functions.Add("UPPER", new SqlServerMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 1 });
            Meta.Functions.Add("LTRIM", new SqlServerMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 1 });
            Meta.Functions.Add("RTRIM", new SqlServerMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 1 });
            Meta.Functions.Add("TRIM", new SqlServerMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCountMin = 1, ParamCountMax = 2 });
            Meta.Functions.Add("LEFT", new SqlServerMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 2 });
            Meta.Functions.Add("RIGHT", new SqlServerMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 2 });
            Meta.Functions.Add("REPLACE", new SqlServerMetaProgrammabilitySignature { DataType = "STR/BIN", ParamCount = 3 });

            Meta.Functions.Add("ISNULL", new SqlServerMetaProgrammabilitySignature { ParamCount = 2 });
            Meta.Functions.Add("NULLIF", new SqlServerMetaProgrammabilitySignature { ParamCount = 2 });
            Meta.Functions.Add("COALESCE", new SqlServerMetaProgrammabilitySignature { ParamCountMin = 2 });
            Meta.Functions.Add("IIF", new SqlServerMetaProgrammabilitySignature { ParamCount = 3 });
            Meta.Functions.Add("FORMATMESSAGE", new SqlServerMetaProgrammabilitySignature { DataType = "NVARCHAR", ParamCountMin = 2 });

            Meta.Functions.Add("YEAR", new SqlServerMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });
            Meta.Functions.Add("MONTH", new SqlServerMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });
            Meta.Functions.Add("DAY", new SqlServerMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });
            Meta.Functions.Add("SIGN", new SqlServerMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });
            Meta.Functions.Add("ISJSON", new SqlServerMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });

            Meta.Functions.Add("COUNT", new SqlServerMetaProgrammabilitySignature { DataType = "INT", ParamCount = 1 });
            Meta.Functions.Add("MAX", new SqlServerMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("AVG", new SqlServerMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("SUM", new SqlServerMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("ROUND", new SqlServerMetaProgrammabilitySignature { ParamCountMin = 1, ParamCountMax = 2 });
            Meta.Functions.Add("CAST", new SqlServerMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("CONVERT", new SqlServerMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("TRY_CAST", new SqlServerMetaProgrammabilitySignature { ParamCount = 1 });
            Meta.Functions.Add("TRY_CONVERT", new SqlServerMetaProgrammabilitySignature { ParamCount = 1 });

            Meta.Functions.Add("NEWID", new SqlServerMetaProgrammabilitySignature { DataType = "UNIQUEIDENTIFIER", ParamCount = 0 });
            Meta.Functions.Add("EVENT_DATA", new SqlServerMetaProgrammabilitySignature { DataType = "XML", ParamCount = 0 });

            Meta.Enums.Add(TSqlDomainAttributes.DateTimePartEnum, new List<SqlServerMetaEnumElementProperties>());
        }

        private void InitEnumInfo()
        {
            var enumInfo = Meta.Enums[TSqlDomainAttributes.DateTimePartEnum];
            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "MS" });
            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "MILLISECOND" });
            enumInfo.First(en => en.Name == "MS").Properties.Add("Alias", "MILLISECOND");
            enumInfo.First(en => en.Name == "MILLISECOND").Properties.Add("Requires", "TIME");

            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "NS" });
            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "NANOSECOND" });
            enumInfo.First(en => en.Name == "NS").Properties.Add("Alias", "NANOSECOND");
            enumInfo.First(en => en.Name == "NANOSECOND").Properties.Add("Requires", "TIMENANO");

            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "S" });
            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "SECOND" });
            enumInfo.First(en => en.Name == "S").Properties.Add("Alias", "SECOND");
            enumInfo.First(en => en.Name == "SECOND").Properties.Add("Requires", "TIME");

            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "MI" });
            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "MINUTE" });
            enumInfo.First(en => en.Name == "MI").Properties.Add("Alias", "MINUTE");
            enumInfo.First(en => en.Name == "MINUTE").Properties.Add("Requires", "TIME");

            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "HH" });
            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "HOUR" });
            enumInfo.First(en => en.Name == "HH").Properties.Add("Alias", "HOUR");
            enumInfo.First(en => en.Name == "HOUR").Properties.Add("Requires", "TIME");

            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "YY" });
            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "YEAR" });
            enumInfo.First(en => en.Name == "YY").Properties.Add("Alias", "YEAR");
            enumInfo.First(en => en.Name == "YEAR").Properties.Add("Requires", "DATE");

            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "M" });
            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "MONTH" });
            enumInfo.First(en => en.Name == "M").Properties.Add("Alias", "MONTH");
            enumInfo.First(en => en.Name == "MONTH").Properties.Add("Requires", "DATE");

            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "DD" });
            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "DAY" });
            enumInfo.First(en => en.Name == "DD").Properties.Add("Alias", "DAY");
            enumInfo.First(en => en.Name == "DAY").Properties.Add("Requires", "DATE");

            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "Q" });
            enumInfo.Add(new SqlServerMetaEnumElementProperties { Name = "QUARTER" });
            enumInfo.First(en => en.Name == "Q").Properties.Add("Alias", "QUARTER");
            enumInfo.First(en => en.Name == "QUARTER").Properties.Add("Requires", "DATE");
        }

        private void InitTypeInfo()
        {
            Meta.Types.Add("GEOGRAPHY", new SqlServerMetaTypeDescription { Name = "GEOGRAPHY", CanBeNativelyCompiled = false });
            Meta.Types.Add("JSON", new SqlServerMetaTypeDescription { Name = "JSON" });
            Meta.Types.Add("SYSNAME", new SqlServerMetaTypeDescription { Name = "SYSNAME", AlsoKnownAs = "NVARCHAR" });
            Meta.Types.Add("NVARCHAR", new SqlServerMetaTypeDescription { Name = "NVARCHAR" });
            Meta.Types.Add("VARCHAR", new SqlServerMetaTypeDescription { Name = "VARCHAR" });
            Meta.Types.Add("NATIONAL VARYING CHARACTER", new SqlServerMetaTypeDescription { Name = "NATIONAL VARYING CHARACTER", AlsoKnownAs = "NVARCHAR", ForceToOriginalName = true });
            Meta.Types.Add("CHARACTER", new SqlServerMetaTypeDescription { Name = "CHARACTER", AlsoKnownAs = "CHAR", ForceToOriginalName = true });
            Meta.Types.Add("XML", new SqlServerMetaTypeDescription { Name = "XML", CanBeNativelyCompiled = false });
            Meta.Types.Add("TIMESTAMP", new SqlServerMetaTypeDescription { Name = "TIMESTAMP", CanBeNativelyCompiled = false, AlsoKnownAs = "ROWVERSION", ForceToOriginalName = true });
            Meta.Types.Add("ROWVERSION", new SqlServerMetaTypeDescription { Name = "ROWVERSION", CanBeNativelyCompiled = false });
            Meta.Types.Add("HIERARCHYID", new SqlServerMetaTypeDescription { Name = "HIERARCHYID", CanBeNativelyCompiled = false });
            Meta.Types.Add("INTEGER", new SqlServerMetaTypeDescription { Name = "INTEGER", AlsoKnownAs = "INT", ForceToOriginalName = true });
            Meta.Types.Add("INT", new SqlServerMetaTypeDescription { Name = "INT" });
            Meta.Types.Add("DEC", new SqlServerMetaTypeDescription { Name = "DEC", AlsoKnownAs = "DECIMAL", ForceToOriginalName = true });
            Meta.Types.Add("DATETIME", new SqlServerMetaTypeDescription { Name = "DATETIME" });
        }
    }
}
