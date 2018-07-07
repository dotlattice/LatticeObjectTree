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
            : this(rootObject, options: default(IObjectTreeOptions)) { }

        /// <summary>
        /// Constructs an object tree with the specified root object and filter on the descendant nodes to include in the tree.
        /// </summary>
        /// <param name="rootObject">the root object of the tree</param>
        /// <param name="nodeFilter">a filter on which descendant nodes will be included in the tree</param>
        public ObjectTree(object rootObject, IObjectTreeNodeFilter nodeFilter)
            : this(CreateRootNode(rootObject, nodeFilter)) { }

        /// <summary>
        /// Constructs an object tree with the specified root object and filter on the descendant nodes to include in the tree.
        /// </summary>
        /// <param name="rootObject">the root object of the tree</param>
        /// <param name="options">(optional) options that control how the object tree is built</param>
        public ObjectTree(object rootObject, IObjectTreeOptions options)
            : this(CreateRootNode(rootObject, options)) { }

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
            var objectTree = rootObject as ObjectTree;
            if (objectTree == null)
            {
                objectTree = new ObjectTree(rootObject, nodeFilter);
            }
            else
            {
                var rootNode = CreateRootNode(objectTree.RootNode, nodeFilter);
                if (rootNode != objectTree.RootNode)
                {
                    objectTree = new ObjectTree(rootNode);
                }
            }
            return objectTree;
        }

        /// <summary>
        /// Creates an object tree for the specified root object and options, or just returns 
        /// the object if it's an <see cref="ObjectTree"/> that already uses these options.
        /// </summary>
        /// <param name="rootObject">the object that represents the root of the tree, or the tree</param>
        /// <param name="options">(optional) options that control how the object tree is built</param>
        /// <returns>the object tree representation of the specified root object with the specified filter applied to its descendant nodes</returns>
        public static ObjectTree Create(object rootObject, IObjectTreeOptions options)
        {
            var objectTree = rootObject as ObjectTree;
            if(objectTree == null)
            {
                objectTree = new ObjectTree(rootObject, options);
            }
            else
            {
                var rootNode = CreateRootNode(objectTree.RootNode, options);
                if (rootNode != objectTree.RootNode)
                {
                    objectTree = new ObjectTree(rootNode);
                }
            }
            return objectTree;
        }

        private static ObjectTreeNode CreateRootNode(object rootObject, IObjectTreeNodeFilter nodeFilter)
        {
            if (nodeFilter != null && rootObject is ObjectTreeNode rootNode)
            {
                var rootNodeSpawnStrategy = rootNode.SpawnStrategy;
                var basicSpawnStrategy = rootNodeSpawnStrategy as BasicObjectTreeSpawnStrategy
                    ?? (rootNodeSpawnStrategy as DuplicateCheckingObjectTreeSpawnStrategy)?.BackingSpawnStrategy as BasicObjectTreeSpawnStrategy;
                if (basicSpawnStrategy != null && basicSpawnStrategy.Options?.NodeFilter == nodeFilter)
                {
                    return CreateRootNode(rootObject, basicSpawnStrategy.Options);
                }
            }
            IObjectTreeOptions options = nodeFilter != null ? new ObjectTreeOptions { NodeFilter = nodeFilter } : default(IObjectTreeOptions);
            return CreateRootNode(rootObject, options);
        }

        private static ObjectTreeNode CreateRootNode(object rootObject, IObjectTreeOptions options)
        {
            var rootNode = rootObject as ObjectTreeNode;
            if (rootNode == null)
            {
                var defaultSpawnStrategy = options?.DefaultSpawnStrategy ?? new DuplicateCheckingObjectTreeSpawnStrategy(options);
                rootNode = defaultSpawnStrategy.CreateRootNode(rootObject);
            }
            else if (options != null)
            {
                var rootNodeSpawnStrategy = rootNode.SpawnStrategy;

                bool canReuseNode;
                if (options.DefaultSpawnStrategy != null)
                {
                    canReuseNode = Equals(rootNodeSpawnStrategy, options.DefaultSpawnStrategy)
                        || rootNodeSpawnStrategy.GetType() == options.DefaultSpawnStrategy.GetType();
                }
                else
                {
                    var basicSpawnStrategy = rootNodeSpawnStrategy as BasicObjectTreeSpawnStrategy
                        ?? (rootNodeSpawnStrategy as DuplicateCheckingObjectTreeSpawnStrategy)?.BackingSpawnStrategy as BasicObjectTreeSpawnStrategy;
                    canReuseNode = (basicSpawnStrategy != null && Equals(basicSpawnStrategy.Options, options));
                }

                if (!canReuseNode)
                {
                    var defaultSpawnStrategy = new DuplicateCheckingObjectTreeSpawnStrategy(options);
                    rootNode = defaultSpawnStrategy.CreateRootNode(rootNode.Value);
                }
            }
            return rootNode;
        }

        #endregion
    }
}
