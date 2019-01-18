using System;
using System.Collections.Generic;
using System.Linq;

namespace Mongrow.Internals
{
    static class StringExtensions
    {
        public static string ListedAs<TItem>(this IEnumerable<TItem> items, Func<TItem, object> getValue, int level = 1)
        {
            const int factor = 4;

            var indentation = new string(' ', factor * level);

            return string.Join(Environment.NewLine,
                items.Select(item => string.Concat(indentation, $"- {getValue(item)}")));
        }
    }
}