﻿using System.Collections.Generic;

namespace Tests_ApiGroupGenerator.Helpers
{
    public static class Extensions
    {
        public static IEnumerable<T> Without<T>(this IEnumerable<T> source, T value)
        {
            foreach (var item in source)
            {
                if (!item?.Equals(value) ?? value is not null)
                {
                    yield return item;
                }
            }
        }
    }
}