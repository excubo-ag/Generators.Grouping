using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Excubo.Generators.Grouping
{
    public partial class GroupingGenerator
    {
        private const string AttributeText = @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
";

        /// <summary>
        /// This generates code for a specific method within a group.
        /// We create
        /// - in the group struct/interface: a method that
        ///   - matches the return type of the method in the parent
        ///   - has either the name of the method in the parent, or the new name provided by the user
        ///   - has the same type parameters as the method in the parent
        ///   - has the same parameters as the method in the parent
        ///   - has the same constraints as the method in the parent
        ///   - has a body that calls the method in the parent (with explicit type parameters, as it might be that not all parameters are inferable)
        /// </summary>
        /// <param name="method">The method to mirror in the group</param>
        /// <returns></returns>
        private string ProcessMethod(GroupedMethod method)
        {
            var returnType = method.Symbol.ReturnType.ToDisplayString();
            var type_parameters = string.Join(", ", method.Symbol.TypeArguments.Select(t => t.Name));
            type_parameters = string.IsNullOrEmpty(type_parameters) ? type_parameters : "<" + type_parameters + ">";
            var constraints = method.Declaration.ConstraintClauses.ToFullString();
            constraints = string.IsNullOrEmpty(constraints) ? constraints : " " + constraints.Trim(' ');
            var parameters = string.Join(", ", method.Symbol.Parameters.Select(p => (p.IsParams ? "params " : "") + p.Type.ToDisplayString() + " " + p.Name));
            var arguments = string.Join(", ", method.Symbol.Parameters.Select(p => p.Name));
            var type_kind = method.Group.TypeKind switch { TypeKind.Class => "class", TypeKind.Interface => "interface", _ => "struct" };
            var method_impl = method.Group.TypeKind == TypeKind.Interface ? string.Empty : $"=> group_internal__parent.{method.Symbol.Name}{type_parameters}({arguments})";
            var innerCode = $@"
{method.Group.DeclaredAccessibility.ToString().ToLowerInvariant()} partial {type_kind} {method.Group.Name}
{{
    public {returnType} {method.TargetName}{type_parameters}({parameters}){constraints}
        {method_impl};
}}";
            return WrapInOuterTypesAndNamespace(innerCode, method.Group);
        }

        /// <summary>
        /// This generates code for each unique Group.
        /// We create
        /// - in the group struct/interface: a private field that will hold the reference to the parent object,
        /// - in the group struct/interface: a constructor that takes a reference to the parent and assigns that to the private field,
        /// - in the containing class: a property of the group struct/interface type that calls the constructor mentioned above.
        /// </summary>
        /// <param name="group_symbol">The struct/interface to hold all group members</param>
        /// <returns></returns>
        private static string ProcessGroup(INamedTypeSymbol group_symbol, INamedTypeSymbol containing_type)
        {
            // we copy the comment on the struct/interface to the auto-generated property
            static bool IsCommentOrWhitespaceKind(SyntaxTrivia trivia)
            {
                return trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia)
                    || trivia.IsKind(SyntaxKind.EndOfDocumentationCommentToken)
                    || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                    || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                    || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                    || trivia.IsKind(SyntaxKind.XmlComment)
                    || trivia.IsKind(SyntaxKind.XmlCommentEndToken)
                    || trivia.IsKind(SyntaxKind.XmlCommentStartToken)
                    || trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                    || trivia.IsKind(SyntaxKind.EndOfLineTrivia);
            }
            var comments_on_group = string.Join("", group_symbol.DeclaringSyntaxReferences[0].GetSyntax().GetLeadingTrivia().Where(t => IsCommentOrWhitespaceKind(t)).Select(t => t.ToFullString()));
            /// The containing type is the methods containing type, i.e. the reference we need to hold in order to be able to execute methods.
            /// If that's equal to the containing type of the <param name="group_symbol"/>,
            ///     then we need to initialize the property with this,
            ///     otherwise with this.group_internal__parent.
            var group_containing_type_is_containing_type = SymbolEqualityComparer.Default.Equals(group_symbol.ContainingType, containing_type);
            var initializer = group_containing_type_is_containing_type ? "this" : "this.group_internal__parent";
            var outer_name = containing_type.Name;
            var outer_type_parameters = string.Join(", ", containing_type.TypeArguments.Select(t => t.Name));
            outer_type_parameters = string.IsNullOrEmpty(outer_type_parameters) ? outer_type_parameters : "<" + outer_type_parameters + ">";
            var outer_full_name = outer_name + outer_type_parameters;
            var type_kind = group_symbol.TypeKind switch { TypeKind.Class => "class", TypeKind.Interface => "interface", _ => "struct" };
            var interfaces = group_symbol.Interfaces;
            var parent_interfaces = group_symbol.ContainingType.Interfaces;
            var interface_to_implement = interfaces.FirstOrDefault(i => parent_interfaces.Any(pi => SymbolEqualityComparer.Default.Equals(i.ContainingType, pi)));
            var interface_implementation = (interface_to_implement == default ? "" : $@"
{interface_to_implement.ContainingType.Name}.{interface_to_implement.Name} {interface_to_implement.ContainingType.Name}.{interface_to_implement.Name.Substring(2)} => {group_symbol.Name.Substring(1)};");
            var field_and_constructor = $@"
private {outer_full_name} group_internal__parent;
public {group_symbol.Name}({outer_full_name} parent) {{ this.group_internal__parent = parent; }}";
            var property = group_symbol.TypeKind == TypeKind.Interface
                ? $@"
{comments_on_group} {group_symbol.Name} {group_symbol.Name.Substring(2)} {{ get; }}"
                : $@"
{comments_on_group} public {group_symbol.Name} {group_symbol.Name.Substring(1)} => new {group_symbol.Name}({initializer});
{interface_implementation}";
            var inner_code = $@"
{group_symbol.DeclaredAccessibility.ToString().ToLowerInvariant()} partial {type_kind} {group_symbol.Name}
{{
    {(group_symbol.TypeKind == TypeKind.Interface ? string.Empty : field_and_constructor)}
}}
{property}
";
            return WrapInOuterTypesAndNamespace(inner_code, group_symbol);
        }

        private static string WrapInOuterTypesAndNamespace(string inner_code, ISymbol group_symbol)
        {
            for (var symbol = group_symbol; symbol.ContainingSymbol != null && symbol.ContainingSymbol is INamedTypeSymbol containing_type; symbol = symbol.ContainingSymbol)
            {
                var accessibility = containing_type.DeclaredAccessibility.ToString().ToLowerInvariant();
                var type_kind = containing_type.TypeKind switch { TypeKind.Class => "class", TypeKind.Interface => "interface", _ => "struct" };
                var type_parameters = string.Join(", ", containing_type.TypeArguments.Select(t => t.Name));
                type_parameters = string.IsNullOrEmpty(type_parameters) ? type_parameters : "<" + type_parameters + ">";
                inner_code = $@"
{accessibility} partial {type_kind} {containing_type.Name}{type_parameters}
{{
    {inner_code}
}}
";
            }
            var namespaceName = group_symbol.ContainingNamespace.ToDisplayString();
            return @$"
namespace {namespaceName}
{{
    {inner_code}
}}
";
        }
    }
}
