using System;
using System.Collections;
using System.Collections.Generic;

namespace LatticeObjectTree
{
    internal static class CollectionUtils
    {
        public static ICollection<T> AsReadOnly<T>(List<T> list)
        {
#if FEATURE_LIST_ASREADONLY
            return list.AsReadOnly();
#else
            return new ReadOnlyCollection<T>(list);
#endif
        }

#if !FEATURE_LIST_ASREADONLY
        private class ReadOnlyCollection<T> : ICollection<T>
        {
            private readonly ICollection<T> collection;

            public ReadOnlyCollection(ICollection<T> collection)
            {
                if (collection == null) throw new ArgumentNullException(nameof(collection));
                this.collection = collection;
            }

            public int Count => collection.Count;
            public bool Contains(T item) => collection.Contains(item);
            public void CopyTo(T[] array, int arrayIndex) => collection.CopyTo(array, arrayIndex);
            public IEnumerator<T> GetEnumerator() => collection.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)collection).GetEnumerator();

            bool ICollection<T>.IsReadOnly => true;
            void ICollection<T>.Add(T item) => throw new NotSupportedException();
            void ICollection<T>.Clear() => throw new NotSupportedException();
            bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
        }
#endif
    }
}
