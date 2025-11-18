using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Variable and parameter declaration detector.
    /// </summary>
    public partial class VariableDeclarationVisitor : TSqlFragmentVisitor
    {
        private readonly IVariableRegistry varRegistry;
        private readonly ISqlTypeResolver typeResolver;

        public VariableDeclarationVisitor(
            IVariableRegistry varRegistry,
            ISqlTypeResolver typeResolver)
        {
            this.varRegistry = varRegistry ?? throw new ArgumentNullException(nameof(varRegistry));
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
        }

        public override void Visit(DeclareVariableElement node)
        {
            string varName = node.VariableName.Value;

            if (varRegistry.IsVariableRegistered(varName))
            {
                // TODO : or still call and report an error?
                return;
            }

            var typeReference = typeResolver.ResolveType(node.DataType);
            if (typeReference is null)
            {
                // unsupported type
                return;
            }

            varRegistry.RegisterVariable(varName, typeReference);
        }
    }
}
