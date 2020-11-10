using System.Collections.Generic;

namespace KioskBrains.Kiosk.Helpers.Collections
{
    public static class EnumerableExtensions
    {
        public static HashSet<TItem> ToHashSet<TItem>(this IEnumerable<TItem> source)
        {
            return new HashSet<TItem>(source);
        }
    }
}