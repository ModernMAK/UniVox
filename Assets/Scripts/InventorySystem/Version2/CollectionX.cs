using System.Collections.Generic;

namespace FlowSystem
{
    public static class CollectionX
    {
        public static bool AddUnique<T>(this ICollection<T> collection, T item)
        {
                
            if (collection.Contains(item)) return false;
                
            collection.Add(item);
            return true;
        }
    }
}