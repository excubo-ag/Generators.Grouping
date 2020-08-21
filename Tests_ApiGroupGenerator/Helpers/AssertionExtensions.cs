using Excubo.Generators.Grouping;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace Tests_ApiGroupGenerator.Helpers
{
    public static class AssertionExtensions
    {
        public static AndConstraint<StringAssertions> BeIgnoringLineEndings(this StringAssertions stringAssertions, string expected)
        {
            return stringAssertions.Subject.Should().Be(expected.NormalizeWhitespace());
        }
    }
}