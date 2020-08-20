using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Excubo.Generators.Grouping
{
    internal static class SourceGeneratorContextExtension
    {
        public static void AddCode(this SourceGeneratorContext context, string hint_name, string code)
        {
            context.AddSource(hint_name, SourceText.From(code.Trim(' ', '\t', '\r', '\n'), Encoding.UTF8));
        }
    }
}