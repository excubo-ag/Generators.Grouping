using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Excubo.Generators.Grouping
{
    internal struct GroupedMethod
    {
        public IMethodSymbol Symbol { get; }
        public MethodDeclarationSyntax Declaration { get; }
        public INamedTypeSymbol Group { get; }
        public string? NewName { get; }
        public string TargetName => NewName ?? Symbol.Name;
        public GroupedMethod(IMethodSymbol symbol, MethodDeclarationSyntax declaration, INamedTypeSymbol group, string? new_name)
        {
            Symbol = symbol;
            Declaration = declaration;
            Group = group;
            NewName = new_name;
        }
    }
}
