using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetDaily
{
    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> collection, Random rng) => collection.ElementAt(rng.Next(0, collection.Count()));
    }
}
