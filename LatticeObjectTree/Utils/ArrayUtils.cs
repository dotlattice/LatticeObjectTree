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
}
