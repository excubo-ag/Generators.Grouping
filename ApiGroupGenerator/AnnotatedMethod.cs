using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Excubo.Generators.Grouping
{
    internal struct AnnotatedMethod
    {
        public IMethodSymbol Symbol { get; }
        public MethodDeclarationSyntax Declaration { get; }
        public AttributeSyntax Attribute { get; }
        public SemanticModel SemanticModel { get; }
        public AnnotatedMethod(IMethodSymbol symbol, MethodDeclarationSyntax declaration, SemanticModel model, AttributeSyntax attribute)
        {
            Symbol = symbol;
            Declaration = declaration;
            SemanticModel = model;
            Attribute = attribute;
        }
    }
}