using System.Collections.Generic;
using System.Linq;

namespace Excubo.Generators.Grouping
{
    internal static class IGroupingExtension
    {
        public static void Deconstruct<TKey, TElement>(this IGrouping<TKey, TElement> grouping, out TKey key, out IEnumerable<TElement> elements)
        {
            key = grouping.Key;
            elements = grouping.AsEnumerable();
        }
    }
}