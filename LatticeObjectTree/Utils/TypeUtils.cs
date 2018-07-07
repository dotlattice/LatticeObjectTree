using System;
using System.Collections.Generic;
using System.Linq;
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
            IEnumerable<PropertyInfo> properties;
#if FEATURE_RUNTIME_MEMBERS
            properties = type.GetRuntimeProperties()
                .Where(p => p.GetMethod?.IsStatic != true && p.SetMethod?.IsStatic != true);
#else
            properties = type.GetProperties()
                .Where(p => p.GetAccessors().Any(a => !a.IsStatic));
#endif
            return properties
                .Where(p => p.CanRead || p.CanWrite);
        }

        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
            IEnumerable<FieldInfo> fields;
#if FEATURE_RUNTIME_MEMBERS
            fields = type.GetRuntimeFields();
#else
            fields = type.GetFields();
#endif
            return fields.Where(f => f.IsPublic)
                .Where(f => !f.IsStatic)
                .Where(f => !f.IsLiteral || f.IsInitOnly);
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

        public static bool IsCollection(Type valueType)
        {
            if (valueType == typeof(string) || IsDictionary(valueType))
            {
                return false;
            }
            return IsAssignableFrom(typeof(System.Collections.IEnumerable), valueType);
        }

        public static bool IsDictionary(Type valueType)
        {
            return IsAssignableFrom(typeof(System.Collections.IDictionary), valueType);
        }
    }
}
