using System;
using System.Collections.Generic;
using System.Reflection;

namespace LatticeObjectTree
{
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
}
