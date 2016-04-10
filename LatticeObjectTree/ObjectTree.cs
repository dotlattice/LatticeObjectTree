using System;
using System.Collections.Generic;
using System.Linq;

namespace LatticeObjectTree
{
    /// <summary>
    /// A tree that represents an object and its descendants through its properties, fields, or other members.
    /// </summary>
    public class ObjectTree
    {
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
        private ObjectTree(ObjectTreeNode rootNode)
        {
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));
            RootNode = rootNode;
        }

        /// <summary>
        /// The root node of the tree.  This will never be null.
        /// </summary>
        public ObjectTreeNode RootNode { get; }

        #region Create

        /// <summary>
        /// Creates an object tree for the specified root object, or just returns the object if it's an <see cref="ObjectTree"/>.
        /// </summary>
        /// <param name="rootObject">the object that represents the root of the tree, or the tree</param>
        /// <returns>the object tree representation of the specified object</returns>
        public static ObjectTree Create(object rootObject)
        {
            var objectTree = rootObject as ObjectTree;
            if (objectTree == null)
            {
                objectTree = new ObjectTree(rootObject);
            }
            return objectTree;
        }

        /// <summary>
        /// Creates an object tree for the specified root object and filter, or just returns 
        /// the object if it's an <see cref="ObjectTree"/> that already uses this filter.
        /// </summary>
        /// <param name="rootObject">the object that represents the root of the tree, or the tree</param>
        /// <param name="nodeFilter">a filter on which descendant nodes will be included in the tree</param>
        /// <returns>the object tree representation of the specified root object with the specified filter applied to its descendant nodes</returns>
        public static ObjectTree Create(object rootObject, IObjectTreeNodeFilter nodeFilter)
        {
            if (nodeFilter == null)
            {
                return Create(rootObject);
            }

            var objectTree = rootObject as ObjectTree;
            if (objectTree != null)
            {
                var rootNode = CreateRootNode(objectTree.RootNode, nodeFilter);
                if (rootNode != objectTree.RootNode)
                {
                    objectTree = new ObjectTree(rootNode);
                }
            }

            if (objectTree == null)
            {
                objectTree = new ObjectTree(rootObject, nodeFilter);
            }

            return objectTree;
        }

        private static ObjectTreeNode CreateRootNode(object rootObject)
        {
            var node = rootObject as ObjectTreeNode;
            if (node == null)
            {
                var defaultSpawnStrategy = new DuplicateCheckingObjectTreeSpawnStrategy();
                node = defaultSpawnStrategy.CreateRootNode(rootObject);
            }
            return node;
        }

        private static ObjectTreeNode CreateRootNode(object rootObject, IObjectTreeNodeFilter nodeFilter)
        {
            if (nodeFilter == null)
            {
                return CreateRootNode(rootObject);
            }

            var rootNode = rootObject as ObjectTreeNode;
            if (rootNode != null)
            {
                var filteredSpawnStrategy = rootNode.SpawnStrategy as FilteredObjectTreeSpawnStrategy;
                if (filteredSpawnStrategy == null || filteredSpawnStrategy.Filter != nodeFilter)
                {
                    filteredSpawnStrategy = new FilteredObjectTreeSpawnStrategy(nodeFilter, rootNode.SpawnStrategy);
                    rootNode = filteredSpawnStrategy.CreateRootNode(rootNode.Value);
                }
            }

            if (rootNode == null)
            {
                var filteredSpawnStrategy = new FilteredObjectTreeSpawnStrategy(nodeFilter);
                rootNode = filteredSpawnStrategy.CreateRootNode(rootObject);
            }
            return rootNode;
        }

        #endregion
    }
}
