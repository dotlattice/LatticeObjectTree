using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree
{
    /// <summary>
    /// A path of the edges from one <see cref="ObjectTreeNode"/> to another.
    /// </summary>
    public class ObjectTreeEdgePath : IEquatable<ObjectTreeEdgePath>
    {
        /// <summary>
        /// Constructs a path from the specified edges.
        /// </summary>
        /// <param name="edges">the edges in the path</param>
        /// <exception cref="ArgumentNullException">if <c>edges</c> is null</exception>
        public ObjectTreeEdgePath(IEnumerable<ObjectTreeEdge> edges)
        {
            if (edges == null) throw new ArgumentNullException(nameof(edges));
            Edges = edges.Where(e => e != null).ToList().AsReadOnly();
        }

        /// <summary>
        /// The edges in this path.
        /// </summary>
        public ICollection<ObjectTreeEdge> Edges { get; }

        /// <summary>
        /// Tries to resolve this path from the specified root object.
        /// </summary>
        /// <param name="rootObject">the root object from which to apply the path</param>
        /// <param name="value">the leaf object that was resolved</param>
        /// <returns>true if the resolution was successful</returns>
        public virtual bool TryResolve(object rootObject, out object value)
        {
            value = null;

            var currentObject = rootObject;
            foreach (var edge in Edges)
            {
                if (!edge.TryResolve(currentObject, out currentObject))
                {
                    return false;
                }
            }

            value = currentObject;
            return true;
        }

        /// <summary>
        /// Returns a string representation of the path with a placeholder variable name for the root node.  
        /// Example: "&lt;root&gt;.ObjectProperty.ListProperty[0].PrimitiveField".
        /// </summary>
        /// <returns>a string representation of the path</returns>
        public override string ToString()
        {
            return ToString(rootVariableName: null);
        }

        /// <summary>
        /// Returns a string representation of the path with the specified variable name for the root object.
        /// </summary>
        /// <param name="rootVariableName">the name of the root variable, or null to use a placeholder of "&lt;root&gt;"</param>
        /// <returns>the string representation of this path</returns>
        public string ToString(string rootVariableName)
        {
            return (rootVariableName ?? "<root>") + string.Join(string.Empty, Edges.Select(node => node.ToString()));
        }

        #region Equality

        /// <summary>
        /// Compares this path to another based on whether the sequence of their edge collections are equal.
        /// </summary>
        /// <param name="obj">the object to compare to this</param>
        /// <returns>true if the object is an edge path and this path's edges are <c>SequenceEqual</c> to the other path's edges</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ObjectTreeEdgePath);
        }

        /// <summary>
        /// Compares this path to another based on whether the sequence of their edge collections are equal.
        /// </summary>
        /// <param name="other">the path to compare to this</param>
        /// <returns>true if this path's edges are <c>SequenceEqual</c> to the other path's edges</returns>
        public bool Equals(ObjectTreeEdgePath other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;

            return this.Edges.SequenceEqual(other.Edges);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hashCode;
            unchecked
            {
                hashCode = Edges.Select(node => node.GetHashCode()).Aggregate(37, (current, nodeHashCode) => (current * 397) ^ nodeHashCode);
            }
            return hashCode;
        }

        /// <inheritdoc />
        public static bool operator ==(ObjectTreeEdgePath left, ObjectTreeEdgePath right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(ObjectTreeEdgePath left, ObjectTreeEdgePath right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
