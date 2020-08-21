using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Linq;

namespace Excubo.Generators.Grouping
{
    public partial class GroupingGenerator
    {
        private const string attributeText = @"
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
        /// - in the group struct: a method that
        ///   - matches the return type of the method in the parent
        ///   - has either the name of the method in the parent, or the new name provided by the user
        ///   - has the same type parameters as the method in the parent
        ///   - has the same parameters as the method in the parent
        ///   - has the same constraints as the method in the parent
        ///   - has a body that calls the method in the parent (with explicit type parameters, as it might be that not all parameters are inferable)
        /// </summary>
        /// <param name="classSymbol">The class that the group is contained in</param>
        /// <param name="structSymbol">The struct to hold all group members</param>
        /// <param name="newName">The optional new name for the method inside the group</param>
        /// <param name="methodToCopy">The method to mirror in the group struct</param>
        /// <returns></returns>
        private string ProcessMethod(GroupedMethod method)
        {
            var returnType = method.Symbol.ReturnType.ToDisplayString();
            var type_parameters = string.Join(", ", method.Symbol.TypeArguments.Select(t => t.Name));
            type_parameters = string.IsNullOrEmpty(type_parameters) ? type_parameters : "<" + type_parameters + ">";
            var constraints = method.Declaration.ConstraintClauses.ToFullString();
            constraints = string.IsNullOrEmpty(constraints) ? constraints : " " + constraints.Trim(' ');
            var parameters = string.Join(", ", method.Symbol.Parameters.Select(p => p.Type.ToDisplayString() + " " + p.Name));
            var arguments = string.Join(", ", method.Symbol.Parameters.Select(p => p.Name));
            var innerCode = $@"
public partial struct {method.Group.Name}
{{
    public {returnType} {method.TargetName}{type_parameters}({parameters}){constraints}
        => group_internal__parent.{method.Symbol.Name}{type_parameters}({arguments});
}}";
            return WrapInOuterTypesAndNamespace(innerCode, method.Group);
        }

        /// <summary>
        /// This generates code for each unique Group.
        /// We create
        /// - in the group struct: a private field that will hold the reference to the parent object,
        /// - in the group struct: a constructor that takes a reference to the parent and assigns that to the private field,
        /// - in the containing class: a property of the group struct type that calls the constructor mentioned above.
        /// </summary>
        /// <param name="structSymbol">The struct to hold all group members</param>
        /// <returns></returns>
        private static string ProcessGroupStruct(INamedTypeSymbol structSymbol, INamedTypeSymbol containing_type)
        {
            /// The containing type is the methods containing type, i.e. the reference we need to hold in order to be able to execute methods.
            /// If that's equal to the containing type of the <param name="structSymbol"/>,
            ///     then we need to initialize the property with this,
            ///     otherwise with this.group_internal__parent.
            var group_containing_type_is_containing_type = SymbolEqualityComparer.Default.Equals(structSymbol.ContainingType, containing_type);
            var initializer = group_containing_type_is_containing_type ? "this" : "this.group_internal__parent";
            var outer_name = containing_type.Name;
            var outer_type_parameters = string.Join(", ", containing_type.TypeArguments.Select(t => t.Name));
            outer_type_parameters = string.IsNullOrEmpty(outer_type_parameters) ? outer_type_parameters : "<" + outer_type_parameters + ">";
            var outer_full_name = outer_name + outer_type_parameters;
            var inner_code = $@"
public partial struct {structSymbol.Name}
{{
    private {outer_full_name} group_internal__parent;
    public {structSymbol.Name}({outer_full_name} parent) {{ this.group_internal__parent = parent; }}
}}
public {structSymbol.Name} {structSymbol.Name.Substring(1)} => new {structSymbol.Name}({initializer});
";
            return WrapInOuterTypesAndNamespace(inner_code, structSymbol);
        }

        private static string WrapInOuterTypesAndNamespace(string inner_code, ISymbol structSymbol)
        {
            for (var symbol = structSymbol; symbol.ContainingSymbol != null && symbol.ContainingSymbol is INamedTypeSymbol containing_type; symbol = symbol.ContainingSymbol)
            {
                var accessibility = containing_type.DeclaredAccessibility.ToString().ToLowerInvariant();
                var type_kind = containing_type.TypeKind == TypeKind.Struct ? "struct" : "class";
                var type_parameters = string.Join(", ", containing_type.TypeArguments.Select(t => t.Name));
                type_parameters = string.IsNullOrEmpty(type_parameters) ? type_parameters : "<" + type_parameters + ">";
                inner_code = $@"
{accessibility} partial {type_kind} {containing_type.Name}{type_parameters}
{{
{inner_code.Indented()}}}
";
            }
            var namespaceName = structSymbol.ContainingNamespace.ToDisplayString();
            return @$"
namespace {namespaceName}
{{
{inner_code.Indented()}}}
";
        }
    }
}
