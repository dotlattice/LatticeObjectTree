using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatticeObjectTree.Comparers
{
    /// <summary>
    /// The default formatter for serializing values from an object tree in a difference object.
    /// </summary>
    public class ObjectTreeValueFormatter : ICustomFormatter
    {
        /// <summary>
        /// A default formatter instance.
        /// </summary>
        public static ObjectTreeValueFormatter Instance { get; } = new ObjectTreeValueFormatter();

        /// <summary>
        /// The default constructor (for subclasses).
        /// </summary>
        protected ObjectTreeValueFormatter() { }

        string ICustomFormatter.Format(string format, object value, IFormatProvider formatProvider)
        {
            return Format(value);
        }

        /// <summary>
        /// Returns a string representation of the specified value.
        /// </summary>
        /// <param name="value">the value to format</param>
        /// <returns>the string representation of the value</returns>
        public string Format(object value)
        { 
            if (value == null) return "null";

            var valueType = value.GetType();
            valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            bool isQuotingNecessary;

            string valueString;
            if (valueType == typeof(float))
            {
                valueString = ((float)value).ToString("R") + 'f';
                isQuotingNecessary = false;
            }
            else if (valueType == typeof(double))
            {
                valueString = ((double)value).ToString("R") + 'd';
                isQuotingNecessary = false;
            }
            else if (valueType == typeof(byte[]))
            {
                valueString = "0x" + BitConverter.ToString((byte[])value).Replace("-", "");
                isQuotingNecessary = false;
            }
            else
            {
                valueString = value.ToString();
                if (valueType == typeof(decimal))
                {
                    valueString += 'm';
                    isQuotingNecessary = false;
                }
                else if (valueType == typeof(bool))
                {
                    valueString = valueString.ToLower();
                    isQuotingNecessary = false;
                }
                else if (valueType.IsEnum)
                {
                    valueString = valueType.Name + "." + valueString;
                    isQuotingNecessary = false;
                }
                else
                {
                    isQuotingNecessary = !IsNumeric(valueType);
                }
            }

            if (isQuotingNecessary)
            {
                // If the string contains any "special" characters, then we'll use the verbatim string literal syntax.
                char[] specialCharacters = new[] { '\'', '"', '\n', '\r', '\t', '\0', '\a', '\b', '\f', '\v' };
                if (valueString.Any(specialCharacters.Contains))
                {
                    valueString = "@\"" + valueString.Replace("\"", "\"\"") + "\"";
                }
                else
                {
                    valueString = "\"" + valueString.Replace(@"\", @"\\") + "\"";
                }
            }

            return valueString;
        }


        private static bool IsNumeric(Type type)
        {
            if (type == null) return false;
            type = Nullable.GetUnderlyingType(type) ?? type;
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.Int16:
                case TypeCode.UInt32:
                case TypeCode.Int32:
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }
    }
}
