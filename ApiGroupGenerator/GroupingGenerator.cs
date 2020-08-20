using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Excubo.Generators.Grouping
{
    [Generator]
    public partial class GroupingGenerator : ISourceGenerator
    {
        public void Initialize(InitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(SourceGeneratorContext context)
        {
            context.AddCode("GroupAttribute", attributeText);

            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
            {
                return;
            }

            var candidate_methods = GetCandidateMethods(receiver, GetCompilation(context));

            var target_methods = candidate_methods
                .SelectMany(WithAnnotations)                  // unroll methods with multiple group attributes into one long list
                .Select(WithGroupInformation)                 // enrich with information about the group
                .Where(m => m != null).Select(m => m!.Value); // take those that have valid group information
            foreach (var method in target_methods)
            {
                GenerateMethod(context, method.Group, method);
            }
            var groups = candidate_methods
                .SelectMany(WithAnnotations)                 // unroll methods with multiple group attributes into one long list
                .SelectMany(Groups)                          // enrich with information about the group
                .Where(m => m != null).Select(m => m!)       // take those that have valid group information
                .Distinct()
                ;
            foreach (var group in groups)
            {
                context.AddCode($"group_{group.Name}", ProcessGroupStruct(group));
            }
        }

        private void GenerateMethod(SourceGeneratorContext context, INamedTypeSymbol group, GroupedMethod method)
        {
            context.AddCode($"group_{method.Group.Name}_{method.Symbol.Name}", ProcessMethod(method));
        }

        private static Compilation GetCompilation(SourceGeneratorContext context)
        {
            var options = (context.Compilation as CSharpCompilation)!.SyntaxTrees[0].Options as CSharpParseOptions;
            var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(attributeText, Encoding.UTF8), options));
            return compilation;
        }

        /// <summary>
        /// Enumerate methods with at least one Group attribute
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="compilation"></param>
        /// <returns></returns>
        private static IEnumerable<Method> GetCandidateMethods(SyntaxReceiver receiver, Compilation compilation)
        {
            var attributeSymbol = compilation.GetTypeByMetadataName("Excubo.Generators.Grouping.GroupAttribute");

            // loop over the candidate methods, and keep the ones that are actually annotated
            foreach (var method_declaration in receiver.CandidateMethods)
            {
                var model = compilation.GetSemanticModel(method_declaration.SyntaxTree);
                var method_symbol = model.GetDeclaredSymbol(method_declaration);
                if (method_symbol is null)
                {
                    continue;
                }
                if (method_symbol.GetAttributes().Any(ad => ad.AttributeClass != null && ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
                {
                    yield return new Method(method_symbol, method_declaration, model);
                }
            }
        }

        /// <summary>
        /// Enumerate all Group attributes for a given method
        /// </summary>
        /// <param name="method">A method</param>
        /// <returns>An enumeration of all group attributes of the given method</returns>
        private static IEnumerable<AnnotatedMethod> WithAnnotations(Method method)
        {
            return method.Declaration.AttributeLists                                                     // in all attribute lists
                .SelectMany(l => l.Attributes)                                                           // list all the attributes
                .Where(a => a.Name.ToString().Equals("Group")                                            // and consider all that are the Group attribute
                || a.Name.ToString().Contains("GroupAttribute"))                                         // (or those who use the Group attribute in the more verbose forms)
                .Select(attribute => new AnnotatedMethod(method.Symbol, method.Declaration, method.SemanticModel, attribute)); // return the attribute together with corresponding method symbol and declaration
        }

        /// <summary>
        /// Enriches the method information with information about the group
        /// </summary>
        /// <param name="method">An annotated method</param>
        /// <returns>Same method with Group annotation information</returns>
        private static GroupedMethod? WithGroupInformation(AnnotatedMethod method)
        {
            if (method.Attribute.ArgumentList is null)
            {
                return null;
            }
            var attribute_arguments = method.Attribute.ArgumentList.Arguments;
            var group_symbol = GetGroupTypeSymbol(method.SemanticModel, attribute_arguments);
            if (group_symbol != null && group_symbol.Value.Symbol is INamedTypeSymbol group_type)
            {
                var new_name = attribute_arguments.Count == 2 ? attribute_arguments.Last().Expression.ToString().Trim('\"') : null;
                return new GroupedMethod(method.Symbol, method.Declaration, group_type, new_name);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Enriches the method information with information about the group
        /// </summary>
        /// <param name="method">An annotated method</param>
        /// <returns>Same method with Group annotation information</returns>
        private static IEnumerable<INamedTypeSymbol> Groups(AnnotatedMethod method)
        {
            if (method.Attribute.ArgumentList is null)
            {
                yield break;
            }
            var attribute_arguments = method.Attribute.ArgumentList.Arguments;
            var group_symbol = GetGroupTypeSymbol(method.SemanticModel, attribute_arguments);
            if (group_symbol == null || group_symbol.Value.Symbol is not INamedTypeSymbol)
            {
                yield break;
            }
            // the group type that is used needs to be a child of the methods containing type.
            // every type that sits between the group type and the methods containing type is considered to be a group type.
            // this enables the situation
            // public partial struct _Outer {
            //      public partial struct _Inner { }
            // }
            // [Group(typeof(_Outer._Inner))] public void Foo() {}
            //
            // _Outer._Inner is a group type by usage in the attribute.
            // If we didn't consider _Outer to be a group type too, there would not be any code to make an instance of _Outer available to the user
            for (var type = group_symbol.Value.Symbol; !SymbolEqualityComparer.Default.Equals(type, method.Symbol.ContainingType) && type is INamedTypeSymbol a_group_type; type = type.ContainingType)
            {
                yield return a_group_type;
            }
        }

        /// <summary>
        /// Identify the struct type that will contain the group's members.
        /// </summary>
        /// <param name="model">The semantic model</param>
        /// <param name="attribute_arguments">the attribute arguments on the method</param>
        /// <returns></returns>
        private static SymbolInfo? GetGroupTypeSymbol(SemanticModel model, SeparatedSyntaxList<AttributeArgumentSyntax> attribute_arguments)
        {
            var first_argument = attribute_arguments.FirstOrDefault();
            if (first_argument == default // The DEV did not yet provide an argument to this group. This can't compile (the attribute requires at least one), so we ignore this.
                || first_argument.Expression is not TypeOfExpressionSyntax typeof_expression) // the first argument doesn't seem to be a typeof(T), so this can't yet be right either.
            {
                return null;
            }
            var type_arg_param = typeof_expression.Type;
            var type_symbol = model.GetSymbolInfo(type_arg_param);
            return type_symbol;
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        internal class SyntaxReceiver : ISyntaxReceiver
        {
            public List<MethodDeclarationSyntax> CandidateMethods { get; } = new List<MethodDeclarationSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // any method with at least one attribute is a candidate for property generation
                if (syntaxNode is MethodDeclarationSyntax memberDeclarationSyntax && memberDeclarationSyntax.AttributeLists.Count > 0)
                {
                    CandidateMethods.Add(memberDeclarationSyntax);
                }
            }
        }
    }
}