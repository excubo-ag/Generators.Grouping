using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;

namespace Excubo.Generators.Grouping
{
    internal static class SourceGeneratorContextExtension
    {
        public static void AddCode(this SourceGeneratorContext context, string hint_name, string code)
        {
            context.AddSource(hint_name.Replace("<", "_").Replace(">", "_"), SourceText.From(code.NormalizeWhitespace(), Encoding.UTF8));
        }
    }
}