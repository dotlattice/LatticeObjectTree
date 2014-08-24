using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatticeObjectTree.CodeGenerators
{
    /// <summary>
    /// Options that change how a C# code generator produces its output.
    /// </summary>
    public class CSharpObjectCodeGeneratorOptions
    {
        /// <summary>
        /// The variable to use for the root of the object tree.  
        /// This will also be used to set references to objects that are in the tree more than once.
        /// </summary>
        public string RootVariableName { get; set; }
    }
}
