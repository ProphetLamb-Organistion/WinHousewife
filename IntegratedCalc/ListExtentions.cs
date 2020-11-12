using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegratedCalc
{
    public static class ListExtentions
    {
        public static IList<T> Distinct<T>(this ICollection<T> self, Func<T, T, bool> equalityComparison)
        {
            var distinct = new List<T>(self.Count);
            foreach (var element in self)
            {
                if (!distinct.Any(x => equalityComparison(element, x)))
                    distinct.Add(element);
            }
            return distinct;
        }

        public static IList<T> RemoveAll<T>(this IList<T> self, Predicate<T> match)
        {
            for (int i = self.Count - 1; i >= 0; i--)
            {
                if (match(self[i]))
                    self.RemoveAt(i);
            }
            return self;
        }

        public static bool AddDistinct<T>(this IList<T> self, T element)
        {
            if (!self.Contains(element))
            {
                self.Add(element);
                return true;
            }
            return false;
        }
    }
}
