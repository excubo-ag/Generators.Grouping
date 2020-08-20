using System.Linq;

namespace Excubo.Generators.Grouping
{
    internal static class StringExtension
    {
        public static string Indented(this string code, string indentation = "    ")
        {
            return string.Join("", code.Split('\n').Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => indentation + c + "\n"));
        }
    }
}
