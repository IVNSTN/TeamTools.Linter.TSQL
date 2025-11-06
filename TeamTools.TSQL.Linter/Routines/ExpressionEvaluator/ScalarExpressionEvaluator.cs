using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public class ScalarExpressionEvaluator
    {
        private static readonly SqlFunctionRegistry FuncReg;
        private readonly ViolationReporter violations;
        private readonly SqlVariableRegistry varReg;

        static ScalarExpressionEvaluator()
        {
            FuncReg = new SqlFunctionRegistry();
            SqlFunctionHandlerFactory.Initialize(FuncReg);
        }

        // TODO : split into initializer and analysis methods
        // TODO : scan single script once for many rules
        // TODO : extract builder
        public ScalarExpressionEvaluator(TSqlBatch batch)
        {
            violations = new ViolationReporter();
            var typeResolver = new SqlTypeResolver();
            var typeConverter = new SqlTypeConverter(typeResolver);
            typeResolver.RegisterTypeHandler(new SqlStrTypeHandler(typeConverter, violations));
            typeResolver.RegisterTypeHandler(new SqlIntTypeHandler(typeConverter, violations));
            typeResolver.RegisterTypeHandler(new SqlBigIntTypeHandler(typeConverter, violations));

            var literalFactory = new SqlLiteralValueFactory(typeResolver);

            varReg = new SqlVariableRegistry(typeConverter, violations);

            var varDeclareVisitor = new VariableDeclarationVisitor(varReg, typeResolver);
            batch.Accept(varDeclareVisitor);

            var exprEvaluator = new SqlExpressionEvaluator(varReg, FuncReg, typeConverter, typeResolver, literalFactory, violations, eval => new VariableConditionalLimitDetector(varReg, varReg, eval, violations));
            var varSetVisitor = new SqlScriptAnalyzer(varReg, exprEvaluator, typeResolver, typeConverter, exprEvaluator.ConditionHander, violations);

            batch.Accept(varSetVisitor);

            varReg.Squash();
        }

        public IEnumerable<SqlViolation> Violations => violations.Violations;

        public SqlValue GetValueAt(string varName, int tokenIndex) => varReg.GetValueAt(varName, tokenIndex);

        public bool ContainsVariable(string varName) => varReg.IsVariableRegistered(varName);
    }
}
