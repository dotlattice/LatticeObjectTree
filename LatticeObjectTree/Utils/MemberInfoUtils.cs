using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LatticeObjectTree
{
    internal static class MemberInfoUtils
    {
        public static bool IsConstantField(FieldInfo field)
        {
            return field.IsLiteral && !field.IsInitOnly;
        }
    }
}
