using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LatticeObjectTree
{
    /// <summary>
    /// A tree that represents an object and its descendants through its properties, fields, or other members.
    /// </summary>
    public class ObjectTree
    {
        private readonly ObjectTreeNode rootNode;

        /// <summary>
        /// Constructs an object tree with the specified root object.
        /// </summary>
        /// <param name="rootObject">the root object of the tree</param>
        public ObjectTree(object rootObject)
            : this(CreateRootNode(rootObject)) { }

        /// <summary>
        /// Constructs an object tree with the specified root object and filter on the descendant nodes to include in the tree.
        /// </summary>
        /// <param name="rootObject">the root object of the tree</param>
        /// <param name="nodeFilter">a filter on which descendant nodes will be included in the tree</param>
        public ObjectTree(object rootObject, IObjectTreeNodeFilter nodeFilter)
            : this(CreateRootNode(rootObject, nodeFilter)) { }

        /// <summary>
        /// Constructs an object tree with the specified root node.
        /// </summary>
        /// <param name="rootNode">the root node of the tree</param>
        /// <exception cref="ArgumentNullException">if the rootNode is null</exception>
        public ObjectTree(ObjectTreeNode rootNode)
        {
            if (rootNode == null) throw new ArgumentNullException("rootNode");
            this.rootNode = rootNode;
        }

        /// <summary>
        /// The root node of the tree.  This will never be null.
        /// </summary>
        public ObjectTreeNode RootNode { get { return rootNode; } }

        #region Create

        /// <summary>
        /// Creates an object tree for the specified root object, or just returns the object if it is an ObjectTree.
        /// </summary>
        /// <param name="rootObject">the object that represents the root of the tree, or the tree</param>
        /// <returns>the object tree representation of the specified root object</returns>
        public static ObjectTree Create(object rootObject)
        {
            var objectTree = rootObject as ObjectTree;
            if (objectTree == null)
            {
                objectTree = new ObjectTree(rootObject);
            }
            return objectTree;
        }

        private static ObjectTreeNode CreateRootNode(object rootObject)
        {
            var node = rootObject as ObjectTreeNode;
            if (node == null)
            {
                node = new ObjectTreeNode(rootObject, new DuplicateCheckingObjectTreeSpawnStrategy());
            }
            return node;
        }

        private static ObjectTreeNode CreateRootNode(object rootObject, IObjectTreeNodeFilter nodeFilter)
        {
            if (nodeFilter == null)
            {
                return CreateRootNode(rootObject);
            }

            var node = rootObject as ObjectTreeNode;
            if (node == null)
            {
                node = new ObjectTreeNode(rootObject, new FilteredObjectTreeSpawnStrategy(nodeFilter));
            }
            else
            {
                var filteredSpawnStrategy = node.SpawnStrategy as FilteredObjectTreeSpawnStrategy;
                if (filteredSpawnStrategy == null || filteredSpawnStrategy.Filter != nodeFilter)
                {
                    filteredSpawnStrategy = new FilteredObjectTreeSpawnStrategy(nodeFilter, node.SpawnStrategy);
                }
                node = new ObjectTreeNode(node.Value, filteredSpawnStrategy);
            }
            return node;
        }

        #endregion
    }
}
