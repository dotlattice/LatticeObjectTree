using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LatticeObjectTree
{
    /// <summary>
    /// An edge in an object tree that connects one node to another.
    /// </summary>
    public class ObjectTreeEdge : IEquatable<ObjectTreeEdge>
    {
        /// <summary>
        /// Constructs an empty edge.
        /// </summary>
        public ObjectTreeEdge()
            : this(member: null, index: default(int?), key: null)
        { }

        /// <summary>
        /// Constructs an edge for the specified member (such as a property or field).
        /// </summary>
        /// <param name="member">the member used to access the node from its parent</param>
        /// <exception cref="ArgumentNullException">if the member is null</exception>
        public ObjectTreeEdge(MemberInfo member)
            : this(member: member, index: default(int?), key: member?.Name)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
        }

        /// <summary>
        /// Constructs an edge for the specified index (such as a list element index).
        /// </summary>
        /// <param name="index">the index of the element in a list</param>
        /// <exception cref="ArgumentOutOfRangeException">if the index is negative</exception>
        public ObjectTreeEdge(int index)
            : this(member: null, index: index, key: index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), index, $"{nameof(index)} cannot be negative");
        }

        /// <summary>
        /// Constructs an edge for the specified key (such as a dictionary entry key).
        /// </summary>
        /// <param name="key">the key of the entry in a dictionary</param>
        public ObjectTreeEdge(object key)
            : this(member: null, index: default(int?), key: key) { }


        /// <summary>
        /// Constructs an edge for the specified member and/or index.
        /// </summary>
        /// <param name="member">the member used to access the node from its parent</param>
        /// <param name="index">the index of the element in a list</param>
        /// <param name="key">the key of the element in a dictionary or list</param>
        protected ObjectTreeEdge(MemberInfo member, int? index, object key)
        {
            this.Member = member;
            this.Index = index;
            this.Key = key;
        }

        /// <summary>
        /// The member of this edge, or null if there is no member associated with this edge.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// The type that all values from this edge must have (as either its actual type or an ancestor of its type), 
        /// or null if there is no known type constraint. For example, this could be a property or field type.
        /// </summary>
        public Type MemberType => PropertyType ?? FieldType;

        /// <summary>
        /// The property that this edge represents, or null if this edge does not represent a property.
        /// </summary>
        public PropertyInfo Property => Member as PropertyInfo;

        /// <summary>
        /// The <c>PropertyType</c> of the <c>Property</c>, or null if the <c>Property</c> is null.
        /// </summary>
        public Type PropertyType => Property?.PropertyType;

        /// <summary>
        /// The field that this edge represents, or null if this edge does not represent a field.
        /// </summary>
        public FieldInfo Field => Member as FieldInfo;

        /// <summary>
        /// The <c>FieldType</c> of the <c>Field</c>, or null if the <c>Field</c> is null.
        /// </summary>
        public Type FieldType => Field?.FieldType;

        /// <summary>
        /// The index to this node (for something like an array or other collection), or null if there is no index associated with this edge.
        /// </summary>
        public int? Index { get; }

        /// <summary>
        /// The key to this node, or null if there is no key associated with this edge.
        /// This may be the same as <see cref="Index"/> or the <see cref="MemberInfo.Name"/> of the <see cref="Member"/>.
        /// </summary>
        public object Key { get; }

        /// <summary>
        /// Tries to resolve this edge on the specified parent object.
        /// </summary>
        /// <param name="parentObject">the object on which to apply this edge</param>
        /// <param name="value">the result, or null if the return value is false</param>
        /// <returns>true if the resolution was successful</returns>
        public virtual bool TryResolve(object parentObject, out object value)
        {
            value = null;

            var node = this;
            if (node.Member != null)
            {
                if (parentObject == null)
                {
                    return false;
                }

                var property = node.Property;
                var field = node.Field;
                try
                {
                    if (property != null)
                    {
                        if (node.Index.HasValue)
                        {
                            value = property.GetValue(parentObject, new object[] { node.Index.Value });
                        }
                        else
                        {
                            value = property.GetValue(parentObject, null);
                        }
                    }
                    else if (field != null)
                    {
                        value = field.GetValue(parentObject);
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
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

                try
                {
                    value = currentEnumerable.Cast<object>().ElementAt(node.Index.Value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return false;
                }
            }
            else if (node.Key != null)
            {
                if (parentObject == null)
                {
                    return false;
                }

                if (parentObject is System.Collections.IDictionary parentDictionary && parentDictionary.Contains(node.Key))
                {
                    value = parentDictionary[node.Key];
                }
                else
                {
                    return false;
                }
            }
            else
            {
                value = parentObject;
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
            if (Member != null)
            {
                return "." + Member.Name;
            }
            else if (Index.HasValue)
            {
                return "[" + Index.Value + "]";
            }
            else if (Key != null)
            {
                return "[" + Key.ToString() + "]";
            }
            else
            {
                return string.Empty;
            }
        }

        #region Equality

        /// <summary>
        /// Compares two edges for equality such that two edges that always resolve to the same value 
        /// from any given parent object are considered equal (even if they were created independently).
        /// </summary>
        /// <param name="obj">the object to compare to this one</param>
        /// <returns>true if this edge is equal to the obj</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ObjectTreeEdge);
        }

        /// <summary>
        /// Compares two edges for equality such that two edges that always resolve to the same value 
        /// from any given parent object are considered equal (even if they were created independently).
        /// </summary>
        /// <param name="other">the edge to compare to this one</param>
        /// <returns>true if this edge is equal to the other edge</returns>
        public virtual bool Equals(ObjectTreeEdge other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;

            return MemberInfoEqualityComparer.Instance.Equals(Member, other.Member)
                && Equals(Index, other.Index);
        }

        /// <summary>
        /// Returns a hash code compatible with the <c>Equals</c> method.
        /// </summary>
        /// <returns>the hash code for this edge</returns>
        public override int GetHashCode()
        {
            int hashCode = 7;
            unchecked
            {
                hashCode = 31 * hashCode + (Member != null ? MemberInfoEqualityComparer.Instance.GetHashCode(Member) : 0);
                hashCode = 31 * hashCode + Index.GetHashCode();
            }
            return hashCode;
        }

        /// <inheritdoc />
        public static bool operator ==(ObjectTreeEdge left, ObjectTreeEdge right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(ObjectTreeEdge left, ObjectTreeEdge right)
        {
            return !Equals(left, right);
        }

        private class MemberInfoEqualityComparer : IEqualityComparer<MemberInfo>
        {
            public static MemberInfoEqualityComparer Instance { get; } = new MemberInfoEqualityComparer();

            private MemberInfoEqualityComparer() { }

            public bool Equals(MemberInfo x, MemberInfo y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                if (object.ReferenceEquals(null, x)) return false;
                if (object.ReferenceEquals(null, y)) return false;
                if (x.GetType() != y.GetType()) return false;

                if (x.Name != y.Name) return false;
                if (x.DeclaringType != y.DeclaringType) return false;
                //if (x.MetadataToken != y.MetadataToken) return false;
                if (x.Module != y.Module) return false;

                return true;
            }

            public int GetHashCode(MemberInfo obj)
            {
                if (obj == null) return 0;

                int hashCode = 7;
                unchecked
                {
                    hashCode = 31 * hashCode + (obj.Name?.GetHashCode() ?? 0);
                    hashCode = 31 * hashCode + (obj.DeclaringType?.GetHashCode() ?? 0);
                    //hashCode = 31 * hashCode + obj.MetadataToken.GetHashCode();
                    hashCode = 31 * hashCode + (obj.Module?.GetHashCode() ?? 0);
                }
                return hashCode;
            }
        }

        #endregion
    }
}
