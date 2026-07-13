using System;
using System.Collections.Generic;

namespace TheFoundation.Runtime
{
    public static class ListExtensions
    {
        public static bool AnyCustom<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null) return false;
        
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (match(list[i])) return true;
            }
            return false;
        }
    }
}
