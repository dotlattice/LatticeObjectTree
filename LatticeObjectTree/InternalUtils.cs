using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LatticeObjectTree
{
    internal static class ArrayUtils
    {
        public static T[] Empty<T>()
        {
            //return Array.Empty<T>();
            return new T[0];
        }
    }

    internal static class TypeUtils
    {
        public static bool IsEnum(Type type)
        {
#if FEATURE_TYPE_INFO
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsPrimitive(Type type)
        {
#if FEATURE_TYPE_INFO
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        public static bool IsValueType(Type type)
        {
#if FEATURE_TYPE_INFO
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
#if FEATURE_RUNTIME_MEMBERS
            return type.GetRuntimeProperties();
#else
            return type.GetProperties();
#endif
        }

        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
#if FEATURE_RUNTIME_MEMBERS
            return type.GetRuntimeFields();
#else
            return type.GetFields();
#endif
        }

        public static bool IsAssignableFrom(Type currentType, Type type)
        {
#if FEATURE_TYPE_INFO
            return currentType.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
#else
            return currentType.IsAssignableFrom(type);
#endif
        }

        public static bool IsNumeric(Type type)
        {
            if (type == null) return false;
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(decimal)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong);
        }
    }

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
