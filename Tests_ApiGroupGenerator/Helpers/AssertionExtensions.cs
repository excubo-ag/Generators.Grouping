using FluentAssertions;
using FluentAssertions.Primitives;

namespace Tests_APIGroupGenerator.Helpers
{
    public static class AssertionExtensions
    {
        public static AndConstraint<StringAssertions> BeIgnoringLineEndings(this StringAssertions stringAssertions, string expected)
        {
            return stringAssertions.Subject.Replace("\r\n", "\n").Trim(' ', '\t', '\r', '\n').Should().Be(expected.Replace("\r\n", "\n").Trim(' ', '\t', '\r', '\n'));
        }
    }
}