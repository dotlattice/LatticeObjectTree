using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree
{
    /// <summary>
    /// An edge in an object tree that connects one node to another.
    /// </summary>
    public interface IObjectTreeEdge
    {
        /// <summary>
        /// The member of this edge, or null if there is no member associated with this edge.
        /// </summary>
        MemberInfo Member { get; }

        /// <summary>
        /// The type that all values from this edge must have (as either its actual type or an ancestor of its type), 
        /// or null if there is no known type constraint. For example, this could be a property or field type.
        /// </summary>
        Type MemberType { get; }

        /// <summary>
        /// The index to this node (for something like an array or other collection), or null if there is no index associated with this edge.
        /// </summary>
        int? Index { get; }

        /// <summary>
        /// Tries to resolve this edge on the specified parent object.
        /// </summary>
        /// <param name="parentObject">the object on which to apply this edge</param>
        /// <param name="value">the result, or null if the return value is false</param>
        /// <returns>true if the resolution was successful</returns>
        bool TryResolve(object parentObject, out object value);

        /// <summary>
        /// Returns the string representation of this edge.
        /// </summary>
        /// <returns>the string representation of this edge</returns>
        string ToString();

        /// <summary>
        /// Compares two edges for equality such that two edges that always resolve to the same value 
        /// from any given parent object are considered equal (even if they were created independently).
        /// </summary>
        /// <param name="obj">the object to compare to this one</param>
        /// <returns>true if this edge is equal to the obj</returns>
        bool Equals(object obj);
    }

    /// <summary>
    /// A default implementation of <c>IObjectTreeEdge</c>.
    /// </summary>
    public class DefaultObjectTreeEdge : IObjectTreeEdge, IEquatable<DefaultObjectTreeEdge>
    {
        private readonly MemberInfo member;
        private readonly int? index;

        /// <summary>
        /// Constructs an empty edge.
        /// </summary>
        public DefaultObjectTreeEdge() { }

        /// <summary>
        /// Constructs an edge for the specified member (such as a property or field).
        /// </summary>
        /// <param name="member">the member used to access the node from its parent</param>
        public DefaultObjectTreeEdge(MemberInfo member)
        {
            this.member = member;
        }

        /// <summary>
        /// Constructs an edge for the specified index (such as a list element index).
        /// </summary>
        /// <param name="index">the index of the element in a list</param>
        public DefaultObjectTreeEdge(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException("index", index, "index cannot be negative");
            this.index = index;
        }

        /// <inheritdoc />
        public MemberInfo Member { get { return member; } }

        /// <inheritdoc />
        public Type MemberType
        {
            get
            {
                return PropertyType ?? FieldType;
            }
        }

        #region Property

        /// <summary>
        /// The property that this edge represents, or null if this edge does not represent a property.
        /// </summary>
        public PropertyInfo Property { get { return member as PropertyInfo; } }

        /// <summary>
        /// The <c>PropertyType</c> of the <c>Property</c>, or null if the <c>Property</c> is null.
        /// </summary>
        public Type PropertyType
        {
            get
            {
                var property = Property;
                return property != null ? property.PropertyType : null;
            }
        }

        #endregion

        #region Field

        /// <summary>
        /// The field that this edge represents, or null if this edge does not represent a field.
        /// </summary>
        public FieldInfo Field { get { return member as FieldInfo; } }

        /// <summary>
        /// The <c>FieldType</c> of the <c>Field</c>, or null if the <c>Field</c> is null.
        /// </summary>
        public Type FieldType
        {
            get
            {
                var field = Field;
                return field != null ? field.FieldType : null;
            }
        }

        #endregion

        /// <inheritdoc />
        public int? Index { get { return index; } }

        /// <inheritdoc />
        public virtual bool TryResolve(object parentObject, out object result)
        {
            result = null;

            var node = this;
            if (node.Member != null)
            {
                if (parentObject == null)
                {
                    return false;
                }

                var property = node.Property;
                var field = node.Field;
                if (property != null)
                {
                    if (node.Index.HasValue)
                    {
                        result = property.GetValue(parentObject, new object[] { node.Index.Value });
                    }
                    else
                    {
                        result = property.GetValue(parentObject, null);
                    }
                }
                else if (field != null)
                {
                    result = field.GetValue(parentObject);
                }
            }
            else if (node.Index.HasValue)
            {
                if (parentObject == null)
                {
                    return false;
                }

                var currentEnumerable = parentObject as System.Collections.IEnumerable;
                if (currentEnumerable == null)
                {
                    return false;
                }
                result = currentEnumerable.Cast<object>().ElementAt(node.Index.Value);
            }
            else
            {
                result = parentObject;
            }

            return true;
        }

        /// <summary>
        /// Returns the member name preceded by a ".", the index in square brackets, 
        /// or an empty string for an edge that has neither a member nor an index.
        /// </summary>
        /// <returns>the string representation of this node</returns>
        public override string ToString()
        {
            if (member != null)
            {
                return "." + member.Name;
            }
            else if (index.HasValue)
            {
                return "[" + index.Value + "]";
            }
            else
            {
                return string.Empty;
            }
        }

        #region Equality

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as DefaultObjectTreeEdge);
        }

        /// <summary>
        /// Compares two edges for equality such that two edges that always resolve to the same value 
        /// from any given parent object are considered equal (even if they were created independently).
        /// </summary>
        /// <param name="other">the edge to compare to this one</param>
        /// <returns>true if this edge is equal to the other edge</returns>
        public bool Equals(DefaultObjectTreeEdge other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;

            return MemberInfoEqualityComparer.Instance.Equals(member, other.Member)
                && Equals(Index, other.Index);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hashCode = 7;
            unchecked
            {
                hashCode = 31 * hashCode + (member != null ? MemberInfoEqualityComparer.Instance.GetHashCode(member) : 0);
                hashCode = 31 * hashCode + Index.GetHashCode();
            }
            return hashCode;
        }

        /// <inheritdoc />
        public static bool operator ==(DefaultObjectTreeEdge left, DefaultObjectTreeEdge right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(DefaultObjectTreeEdge left, DefaultObjectTreeEdge right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Compares two <c>MemberInfo</c> objects for equality.
        /// </summary>
        private class MemberInfoEqualityComparer : IEqualityComparer<MemberInfo>
        {
            public static readonly MemberInfoEqualityComparer Instance = new MemberInfoEqualityComparer();
            private MemberInfoEqualityComparer() { }

            public bool Equals(MemberInfo x, MemberInfo y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(null, x)) return false;
                if (ReferenceEquals(null, y)) return false;
                if (x.GetType() != y.GetType()) return false;

                if (x.Name != y.Name) return false;
                if (x.DeclaringType != y.DeclaringType) return false;
                if (x.MetadataToken != y.MetadataToken) return false;
                if (x.Module != y.Module) return false;

                return true;
            }

            public int GetHashCode(MemberInfo obj)
            {
                if (obj == null) return 0;

                int hashCode = 7;
                unchecked
                {
                    hashCode = 31 * hashCode + (obj.Name != null ? obj.Name.GetHashCode() : 0);
                    hashCode = 31 * hashCode + (obj.DeclaringType != null ? obj.DeclaringType.GetHashCode() : 0);
                    hashCode = 31 * hashCode + obj.MetadataToken.GetHashCode();
                    hashCode = 31 * hashCode + (obj.Module != null ? obj.Module.GetHashCode() : 0);
                }
                return hashCode;
            }
        }

        #endregion
    }
}
