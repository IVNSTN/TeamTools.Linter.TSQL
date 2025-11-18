using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator
{
    public class ScalarExpressionEvaluator
    {
        private static readonly SqlFunctionRegistry FuncReg;
        private readonly ViolationReporter violations;
        private readonly SqlVariableRegistry varReg;
        private readonly Func<SqlExpressionEvaluator, IConditionalFlowHandler> conditionEvalMaker;

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
            var literalFactory = new SqlLiteralValueFactory(typeResolver);

            // TODO : Make type handlers and registration static
            typeResolver.RegisterTypeHandler(new SqlStrTypeHandler(typeConverter, violations));
            typeResolver.RegisterTypeHandler(new SqlIntTypeHandler(typeConverter, violations));
            typeResolver.RegisterTypeHandler(new SqlBigIntTypeHandler(typeConverter, violations));

            varReg = new SqlVariableRegistry(typeConverter, violations);

            var varDeclareVisitor = new VariableDeclarationVisitor(varReg, typeResolver);
            batch.Accept(varDeclareVisitor);

            conditionEvalMaker = new Func<SqlExpressionEvaluator, IConditionalFlowHandler>(MakeConditionEvaluator);
            var exprEvaluator = new SqlExpressionEvaluator(varReg, FuncReg, typeConverter, typeResolver, literalFactory, violations, conditionEvalMaker);
            var varSetVisitor = new SqlScriptAnalyzer(varReg, exprEvaluator, typeResolver, typeConverter, exprEvaluator.ConditionHander, violations);

            batch.AcceptChildren(varSetVisitor);

            varReg.Squash();
        }

        public List<SqlViolation> Violations => violations.Violations;

        public static bool IsBatchInteresting(TSqlBatch node)
        {
            if (node.Statements.Count == 1)
            {
                var firstStatement = node.Statements[0];

                if (firstStatement is ReturnStatement
                || firstStatement is PrintStatement
                || firstStatement is ExecuteStatement
                || firstStatement is CreatePartitionFunctionStatement
                || firstStatement is CreatePartitionSchemeStatement
                || firstStatement is CreateRoleStatement
                || firstStatement is CreateUserStatement
                || firstStatement is CreateLoginStatement
                || firstStatement is CreateQueueStatement
                || firstStatement is CreateContractStatement
                || firstStatement is CreateServiceStatement
                || firstStatement is CreateMessageTypeStatement
                || firstStatement is CreateBrokerPriorityStatement
                || firstStatement is CreateSynonymStatement
                || firstStatement is CreateStatisticsStatement
                || firstStatement is CreateSchemaStatement
                || firstStatement is CreateSecurityPolicyStatement
                || firstStatement is DropTableStatement
                || firstStatement is SecurityStatement)
                {
                    // there is nothing interesting for scalar expression evaluator
                    return false;
                }
            }

            return true;
        }

        public SqlValue GetValueAt(string varName, int tokenIndex) => varReg.GetValueAt(varName, tokenIndex);

        public bool ContainsVariable(string varName) => varReg.IsVariableRegistered(varName);

        private IConditionalFlowHandler MakeConditionEvaluator(SqlExpressionEvaluator eval)
        {
            return new VariableConditionalLimitDetector(varReg, varReg, eval, violations);
        }
    }
}
