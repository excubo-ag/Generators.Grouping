using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Excubo.Generators.Grouping
{
    internal struct Method
    {
        public IMethodSymbol Symbol { get; }
        public MethodDeclarationSyntax Declaration { get; }
        public SemanticModel SemanticModel { get; }
        public Method(IMethodSymbol symbol, MethodDeclarationSyntax declaration, SemanticModel model)
        {
            Symbol = symbol;
            Declaration = declaration;
            SemanticModel = model;
        }
    }
}