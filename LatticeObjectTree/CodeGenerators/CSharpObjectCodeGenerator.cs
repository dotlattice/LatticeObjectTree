using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LatticeObjectTree.CodeGenerators
{
    /// <summary>
    /// Generates C# code from an object.
    /// </summary>
    /// <remarks>
    /// This won't always generate valid or complete code.  It's intended to at least get close, though.
    /// </remarks>
    public class CSharpObjectCodeGenerator
    {
        private readonly CSharpObjectCodeGeneratorOptions options;

        /// <summary>
        /// Constructs a generator, optionally with the specified options.
        /// </summary>
        /// <param name="options">the options on how to generate the code</param>
        public CSharpObjectCodeGenerator(CSharpObjectCodeGeneratorOptions options = null)
        {
            this.options = options ?? new CSharpObjectCodeGeneratorOptions();
        }

        /// <summary>
        /// Generates code for the specified object.
        /// </summary>
        /// <param name="obj">the object for which to generate code</param>
        /// <returns>the C# code for creating the specified object</returns>
        public string GenerateCode(object obj)
        {
            return GenerateCodeForObjectTree(ObjectTree.Create(obj));
        }

        /// <summary>
        /// Generates code for the specified object tree.
        /// </summary>
        /// <param name="objectTree">the tree for which to generate code</param>
        /// <returns>the C# for creating the object represented by the tree</returns>
        public string GenerateCodeForObjectTree(ObjectTree objectTree)
        {
            using (var stringWriter = new StringWriter())
            {
                WriteCodeForObjectTree(stringWriter, objectTree);
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Writes the code for the specified object tree to the specified <c>TextWriter</c>.
        /// </summary>
        /// <param name="writer">the writer to which to write the generated C# code</param>
        /// <param name="objectTree">the tree that represents the object for which to generate the C# code</param>
        public void WriteCodeForObjectTree(TextWriter writer, ObjectTree objectTree)
        {
            if (!string.IsNullOrWhiteSpace(options.RootVariableName))
            {
                writer.Write("var {0} = ", options.RootVariableName);
            }
            WriteCodeForObjectTreeNodeRecursive(writer, objectTree.RootNode);
        }

        private void WriteCodeForObjectTreeNodeRecursive(TextWriter writer, ObjectTreeNode node, int level = 0)
        {
            // Protection from inifinite recursion
            const int maxLevel = 1000;
            if (level > maxLevel)
            {
                throw new InvalidOperationException(string.Format("Exceeded max nested object level of {0}", maxLevel));
            }

            var indent = level > 0 ? new String('\t', level) : string.Empty;

            //writer.Write(indent);
            writer.Write(GenerateCodeForObject(node.Value, propertyType: node.EdgeFromParent != null ? node.EdgeFromParent.MemberType : null));

            var childNodes = node.ChildNodes.ToList();
            var usefulChildNodes = childNodes.Where(n => !IsObjectDefault(n.Value) && IsEdgeWritable(n.EdgeFromParent));
            if (usefulChildNodes.Any())
            {
                writer.WriteLine();
                writer.Write(indent);
                writer.Write("{");
                writer.WriteLine();
                foreach (var childNode in usefulChildNodes)
                {
                    writer.Write(indent);
                    writer.Write('\t');
                    WriteCodeForEdge(writer, childNode.EdgeFromParent);

                    if (childNode.OriginalNode != null)
                    {
                        // For a duplicate node, we'll just print the path back to the original node.
                        var originalNode = childNode.OriginalNode;
                        var originalMemberPathString = originalNode.ToEdgePath().ToString(options.RootVariableName);
                        writer.Write(originalMemberPathString);
                        writer.Write(",");
                    }
                    else
                    {
                        WriteCodeForObjectTreeNodeRecursive(writer, childNode, level: level + 1);
                    }
                    writer.WriteLine();
                }
                writer.Write(indent);
                writer.Write("}");
            }
            if (level > 0)
            {
                writer.Write(",");
            }
        }

        private static bool IsObjectDefault(object obj)
        {
            return Equals(obj, GetDefaultForObject(obj));
        }

        private static object GetDefaultForObject(object obj)
        {
            if (obj == null) return null;

            var objType = obj.GetType();
            return objType.IsValueType ? Activator.CreateInstance(objType) : (object)null;
        }

        private static bool IsEdgeWritable(IObjectTreeEdge edge)
        {
            if (edge == null) return false;

            var property = edge.Member as PropertyInfo;
            if (property != null)
            {
                return property.CanWrite && property.GetSetMethod() != null;
            }

            return true;
        }

        private void WriteCodeForEdge(TextWriter writer, IObjectTreeEdge edge)
        {
            if (edge.Member is PropertyInfo)
            {
                var property = edge.Member as PropertyInfo;
                writer.Write(property.Name);
                if (edge.Index.HasValue)
                {
                    writer.Write('[');
                    writer.Write(edge.Index.Value);
                    writer.Write(']');
                }
                writer.Write(" = ");
            }
            else if (edge.Member is FieldInfo)
            {
                var field = edge.Member as FieldInfo;
                writer.Write(field.Name);
                writer.Write(" = ");
            }
        }

        private static string GenerateCodeForObject(object value, Type propertyType)
        {
            if (value == null)
            {
                return "null";
            }

            string valueString;
            var valueType = value.GetType();
            if (valueType == typeof(string))
            {
                valueString = (string)value;

                // If the string contains any "special" characters, then we'll use the verbatim string literal syntax.
                char[] specialCharacters = new[] { '\'', '"', '\n', '\r', '\t', '\0', '\a', '\b', '\f', '\v' };
                if (valueString.Any(specialCharacters.Contains))
                {
                    valueString = "@\"" + valueString.Replace("\"", "\"\"") + "\"";
                }
                else
                {
                    valueString = "\"" + valueString + "\"";
                }
            }
            else if (valueType.IsValueType)
            {
                valueString = GenerateCodeForValueType(value);
            }
            else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(valueType))
            {
                valueString = GenerateConstructorForEnumerable((System.Collections.IEnumerable)value, propertyType);
            }
            else
            {
                valueString = GenerateConstructorForObject(value);
            }

            return valueString;
        }

        private static string GenerateCodeForValueType(object value)
        {
            var valueType = value.GetType();
            valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            // By default, we'll just use the normal "ToString" method to convert the thing.
            // Then we'll go through and do some special handling for the rest.

            string valueString;
            if (valueType == typeof(float))
            {
                // Need to use the round-trip format specified to make sure we don't lose any data.
                // http://msdn.microsoft.com/en-us/library/dwhawy9k.aspx?ppud=4#RFormatString
                valueString = ((float)value).ToString("R") + 'f';
            }
            else if (valueType == typeof(double))
            {
                // Need to use the round-trip format specified to make sure we don't lose any data.
                // http://msdn.microsoft.com/en-us/library/dwhawy9k.aspx?ppud=4#RFormatString
                valueString = ((double)value).ToString("R") + 'd';
            }
            else if (valueType == typeof(DateTime))
            {
                var dtValue = (DateTime)value;

                var dtStringBuilder = new StringBuilder();
                dtStringBuilder.AppendFormat("new DateTime({0}, {1}, {2}", dtValue.Year, dtValue.Month, dtValue.Day);
                if (dtValue.TimeOfDay != TimeSpan.Zero)
                {
                    dtStringBuilder.AppendFormat(", {0}, {1}, {2}", dtValue.Hour, dtValue.Minute, dtValue.Second);
                    if (dtValue.Millisecond != 0)
                    {
                        dtStringBuilder.AppendFormat(", {0}", dtValue.Millisecond);
                    }
                }
                dtStringBuilder.Append(")");
                valueString = dtStringBuilder.ToString();
            }
            else if (valueType == typeof(DateTimeOffset))
            {
                var dtoValue = (DateTimeOffset)value;

                var dtoStringBuilder = new StringBuilder();
                dtoStringBuilder.AppendFormat("new DateTimeOffset({0}, {1}, {2}, {3}, {4}, {5}",
                    dtoValue.Year, dtoValue.Month, dtoValue.Day,
                    dtoValue.Hour, dtoValue.Minute, dtoValue.Second
                );
                if (dtoValue.Millisecond != 0)
                {
                    dtoStringBuilder.AppendFormat(", {0}", dtoValue.Millisecond);
                }
                if (dtoValue.Offset.Hours == 0)
                {
                    dtoStringBuilder.Append(", TimeSpan.Zero");
                }
                else
                {
                    dtoStringBuilder.AppendFormat(", TimeSpan.FromHours({0})", dtoValue.Offset.Hours);
                }
                dtoStringBuilder.Append(")");

                valueString = dtoStringBuilder.ToString();
            }
            else if (valueType == typeof(TimeSpan))
            {
                var timeSpanValue = (TimeSpan)value;

                var timeSpanStringBuilder = new StringBuilder();
                timeSpanStringBuilder.Append("new TimeSpan(");
                if (timeSpanValue.Days != 0 || timeSpanValue.Milliseconds != 0)
                {
                    timeSpanStringBuilder.AppendFormat("{0}, ", timeSpanValue.Days);
                }
                timeSpanStringBuilder.AppendFormat("{0}, {1}, {2}", timeSpanValue.Hours, timeSpanValue.Minutes, timeSpanValue.Seconds);
                if (timeSpanValue.Milliseconds != 0)
                {
                    timeSpanStringBuilder.AppendFormat(", {0}", timeSpanValue.Milliseconds);
                }
                timeSpanStringBuilder.Append(")");
                valueString = timeSpanStringBuilder.ToString();
            }
            else
            {
                valueString = value.ToString();
                if (valueType == typeof(decimal))
                {
                    valueString += 'm';
                }
                else if (valueType == typeof(bool))
                {
                    valueString = valueString.ToLower();
                }
                else if (valueType.IsEnum)
                {
                    valueString = valueType.Name + "." + valueString;
                }
                else if (valueType == typeof(Guid))
                {
                    valueString = "new Guid(\"" + valueString + "\")";
                }
            }

            return valueString;
        }

        private static string GenerateConstructorForEnumerable(System.Collections.IEnumerable value, Type propertyType)
        {
            var valueType = value.GetType();

            propertyType = propertyType ?? valueType;

            Type elementType = null;
            if (valueType.IsArray)
            {
                elementType = valueType.GetElementType();
            }

            if (elementType != null)
            {
                if (!value.Cast<object>().Any())
                {
                    return string.Format("new {0}[0]", elementType.Name);
                }
                else
                {
                    return string.Format("new {0}[]", elementType.Name);
                }
            }

            return GenerateConstructorForObject(value);
        }

        private static string GenerateConstructorForObject(object value)
        {
            var valueType = value.GetType();

            var constructor = valueType.GetConstructors().OrderBy(x => x.GetParameters().Length).FirstOrDefault();
            if (constructor == null)
            {
                return "default(" + valueType + ")";
            }

            var sb = new StringBuilder();
            sb.AppendFormat("new {0}(", GenerateTypeName(valueType));

            bool isLeadingCommaNecessary = false;
            foreach (var parameter in constructor.GetParameters())
            {
                if (isLeadingCommaNecessary)
                {
                    sb.Append(", ");
                }
                else
                {
                    isLeadingCommaNecessary = true;
                }

                var parameterName = parameter.Name;
                sb.Append(parameterName + ": ");

                PropertyInfo property;
                try
                {
                    property = valueType.GetProperty(parameterName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                }
                catch (AmbiguousMatchException)
                {
                    // If there's two properties with the same name (when ignoring case), then we'll try again 
                    // and order it so we get the property that is the closest match.
                    property = valueType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(p => GetNumberOfCharacterMatches(p.Name, parameterName))
                        .FirstOrDefault();
                }

                if (property != null && property.CanRead && !property.GetIndexParameters().Any())
                {
                    var childValue = property.GetValue(value, null);
                    var childValueCode = GenerateCodeForObject(childValue, propertyType: property.PropertyType);
                    sb.Append(childValueCode);
                }
                else
                {
                    sb.AppendFormat("default({0})", GenerateTypeName(parameter.ParameterType));
                }
            }
            sb.Append(")");

            return sb.ToString();
        }

        private static string GenerateTypeName(Type type)
        {
            if (type.IsArray)
            {
                return string.Format("{0}[]", GenerateTypeName(type.GetElementType()));
            }

            string typeName = null;
            // This is to get fancier names for some types (like "int" instead of "System.Int32").
            if (type.Namespace == "System" && (type.IsValueType || type == typeof(string)))
            {
                using (var provider = new Microsoft.CSharp.CSharpCodeProvider())
                {
                    typeName = provider.GetTypeOutput(new System.CodeDom.CodeTypeReference(type));
                }
            }
            typeName = typeName ?? type.Name;

            var genericArguments = type.GetGenericArguments();
            if (!genericArguments.Any())
            {
                return typeName;
            }

            var modifiedTypeName = typeName.Substring(0, typeName.IndexOf("`"));
            return modifiedTypeName + "<" + string.Join(",", genericArguments.Select(GenerateTypeName)) + ">";
        }

        private static int GetNumberOfCharacterMatches(string a, string b)
        {
            if (a == null || b == null) return 0;
            return a.Zip(b, (charA, charB) => charA == charB ? 1 : 0).Sum();
        }
    }
}
