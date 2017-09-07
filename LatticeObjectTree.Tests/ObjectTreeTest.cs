using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree
{
    public class ObjectTreeTest
    {
        [Test]
        public void Construct_NullValue()
        {
            var tree = new ObjectTree(default(object));
            Assert.IsNull(tree.RootNode.Value);
            Assert.AreEqual(0, tree.RootNode.ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Unknown, tree.RootNode.NodeType);
        }

        [Test]
        public void Construct_NodeValueToObjectConstructor()
        {
            var node = new ObjectTreeNode("test", ObjectTreeNodeType.Primitive);
            var tree = new ObjectTree(node);
            Assert.AreSame(node, tree.RootNode);
            Assert.AreEqual("test", tree.RootNode.Value);
            Assert.AreEqual(0, tree.RootNode.ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Primitive, tree.RootNode.NodeType);
        }

        [Test]
        public void Create_ObjectTree()
        {
            var tree1 = new ObjectTree("test");
            var tree2 = ObjectTree.Create(tree1);
            Assert.AreSame(tree1, tree2);
            Assert.AreEqual("test", tree2.RootNode.Value);
            Assert.AreEqual(0, tree2.RootNode.ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Primitive, tree2.RootNode.NodeType);
        }

        [Test]
        public void CreateWithFilter_UnfilteredObjectTree()
        {
            var tree = new ObjectTree("test");

            var filter = new ObjectTreeNodeFilter { ExcludedPropertyNames = new[] { "PropertyName" } };
            var filteredTree = ObjectTree.Create(tree, filter);

            Assert.AreNotSame(tree, filteredTree);
            Assert.AreEqual("test", filteredTree.RootNode.Value);
            Assert.AreEqual(0, filteredTree.RootNode.ChildNodes.Count());

            Assert.IsInstanceOf<FilteredObjectTreeSpawnStrategy>(filteredTree.RootNode.SpawnStrategy);
            Assert.AreSame(filter, ((FilteredObjectTreeSpawnStrategy)filteredTree.RootNode.SpawnStrategy).Filter);
        }

        [Test]
        public void CreateWithFilter_ObjectTreeWithSameFilter()
        {
            var filter = new ObjectTreeNodeFilter { ExcludedPropertyNames = new[] { "PropertyName" } };
            var tree = new ObjectTree("test", filter);

            var filteredTree = ObjectTree.Create(tree, filter);

            Assert.AreSame(tree, filteredTree);
            Assert.AreEqual("test", filteredTree.RootNode.Value);
            Assert.AreEqual(0, filteredTree.RootNode.ChildNodes.Count());

            Assert.IsInstanceOf<FilteredObjectTreeSpawnStrategy>(filteredTree.RootNode.SpawnStrategy);
            Assert.AreSame(filter, ((FilteredObjectTreeSpawnStrategy)filteredTree.RootNode.SpawnStrategy).Filter);
        }

        [Test]
        public void CreateWithFilter_ObjectTreeWithDifferentFilter()
        {
            var originalFilter = new ObjectTreeNodeFilter { ExcludedPropertyNames = new[] { "PropertyName" } };
            var tree = new ObjectTree("test", originalFilter);

            var newFilter = new ObjectTreeNodeFilter { ExcludedPropertyNames = new[] { "PropertyName" } };
            var filteredTree = ObjectTree.Create(tree, newFilter);

            Assert.AreNotSame(tree, filteredTree);
            Assert.AreEqual("test", filteredTree.RootNode.Value);
            Assert.AreEqual(0, filteredTree.RootNode.ChildNodes.Count());

            Assert.IsInstanceOf<FilteredObjectTreeSpawnStrategy>(filteredTree.RootNode.SpawnStrategy);
            Assert.AreSame(newFilter, ((FilteredObjectTreeSpawnStrategy)filteredTree.RootNode.SpawnStrategy).Filter);
        }

        [Test]
        public void Construct_SampleObject1_Complete()
        {
            var obj = new SampleObject1
            {
                Id = 2,
                Name = "Test",
                ChildArray = new SampleChildObject1[] 
                {
                    new SampleChildObject1
                    {
                        Hi = 3,
                    },
                },
                ChildCollection = new List<SampleChildObject1>()
                {
                    new SampleChildObject1
                    {
                        Hi = 1,
                    }
                },
            };

            var objectTree = new ObjectTree(obj);

            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj, rootNode.Value);

            var childNodes = objectTree.RootNode.ChildNodes.ToList();
            Assert.AreEqual(4, childNodes.Count);

            Assert.AreEqual("Id", GetEdgeFromNode(childNodes.ElementAt(0)).Property.Name);
            Assert.AreEqual(2, childNodes.ElementAt(0).Value);
            Assert.AreEqual("Name", GetEdgeFromNode(childNodes.ElementAt(1)).Property.Name);
            Assert.AreEqual("Test", childNodes.ElementAt(1).Value);
            Assert.AreSame(obj.ChildArray, childNodes.ElementAt(2).Value);
            Assert.AreSame(obj.ChildCollection, childNodes.ElementAt(3).Value);

            Assert.AreEqual(0, childNodes.ElementAt(0).ChildNodes.Count());
            Assert.AreEqual(0, childNodes.ElementAt(1).ChildNodes.Count());
            Assert.AreEqual(1, childNodes.ElementAt(2).ChildNodes.Count());
            Assert.AreEqual(1, childNodes.ElementAt(3).ChildNodes.Count());

            var arrayChild = childNodes.ElementAt(2).ChildNodes.Single();
            Assert.AreSame(obj.ChildArray.Single(), arrayChild.Value);
            Assert.AreEqual(1, arrayChild.ChildNodes.Count());
            Assert.AreEqual("Hi", GetEdgeFromNode(arrayChild.ChildNodes.Single()).Property.Name);
            Assert.AreEqual(3, arrayChild.ChildNodes.Single().Value);

            var listChild = childNodes.ElementAt(3).ChildNodes.Single();
            Assert.AreSame(obj.ChildCollection.Single(), listChild.Value);
            Assert.AreEqual(1, listChild.ChildNodes.Count());
            Assert.AreEqual("Hi", GetEdgeFromNode(listChild.ChildNodes.Single()).Property.Name);
            Assert.AreEqual(1, listChild.ChildNodes.Single().Value);
        }

        [Test]
        public void Construct_SampleObject1_OnlyEmpyChildCollections()
        {
            var obj = new SampleObject1
            {
                ChildArray = new SampleChildObject1[0],
                ChildCollection = new List<SampleChildObject1>(),
            };

            var objectTree = new ObjectTree(obj);

            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj, rootNode.Value);

            var childNodes = objectTree.RootNode.ChildNodes.ToList();
            Assert.AreEqual(4, childNodes.Count);

            Assert.AreEqual(default(int), childNodes.ElementAt(0).Value);
            Assert.IsNull(childNodes.ElementAt(1).Value);
            Assert.AreSame(obj.ChildArray, childNodes.ElementAt(2).Value);
            Assert.AreSame(obj.ChildCollection, childNodes.ElementAt(3).Value);

            Assert.AreEqual(0, childNodes.ElementAt(0).ChildNodes.Count());
            Assert.AreEqual(0, childNodes.ElementAt(1).ChildNodes.Count());
            Assert.AreEqual(0, childNodes.ElementAt(2).ChildNodes.Count());
            Assert.AreEqual(0, childNodes.ElementAt(3).ChildNodes.Count());
        }

        [Test]
        public void Construct_SampleObject2_FieldsAndProperties()
        {
            var obj = new SampleObject2
            {
                id = 1,
                Id = 2,
                name = "hello",
                Name = "World",
            };
            var objectTree = new ObjectTree(obj);


            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj, rootNode.Value);

            var childNodes = objectTree.RootNode.ChildNodes.ToList();
            Assert.AreEqual(4, childNodes.Count);

            Assert.AreEqual("Id", GetEdgeFromNode(childNodes.ElementAt(0)).Property.Name);
            Assert.AreEqual(2, childNodes.ElementAt(0).Value);
            Assert.AreEqual(0, childNodes.ElementAt(0).ChildNodes.Count());

            Assert.AreEqual("Name", GetEdgeFromNode(childNodes.ElementAt(1)).Property.Name);
            Assert.AreEqual("World", childNodes.ElementAt(1).Value);
            Assert.AreEqual(0, childNodes.ElementAt(1).ChildNodes.Count());

            Assert.AreEqual("id", GetEdgeFromNode(childNodes.ElementAt(2)).Field.Name);
            Assert.AreEqual(1, childNodes.ElementAt(2).Value);
            Assert.AreEqual(0, childNodes.ElementAt(2).ChildNodes.Count());

            Assert.AreEqual("name", GetEdgeFromNode(childNodes.ElementAt(3)).Field.Name);
            Assert.AreEqual("hello", childNodes.ElementAt(3).Value);
            Assert.AreEqual(0, childNodes.ElementAt(3).ChildNodes.Count());
        }

        [Test]
        public void Construct_SampleObject3_FieldsAndPropertiesSubclass()
        {
            var obj = new SampleObject3
            {
                id = 1,
                Id = 2,
                name = "hello",
                Name = "World",
                createdDate = new DateTime(2000, 1, 1),
                CreatedDate = new DateTime(2000, 1, 1),
            };
            var objectTree = new ObjectTree(obj);


            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj, rootNode.Value);

            var childNodes = objectTree.RootNode.ChildNodes.ToList();
            Assert.AreEqual(6, childNodes.Count);

            int currentIndex = 0;

            Assert.AreEqual("CreatedDate", GetEdgeFromNode(childNodes.ElementAt(currentIndex)).Property.Name);
            Assert.AreEqual(new DateTime(2000, 1, 1), childNodes.ElementAt(currentIndex).Value);
            Assert.AreEqual(0, childNodes.ElementAt(currentIndex).ChildNodes.Count());
            currentIndex++;

            Assert.AreEqual("Id", GetEdgeFromNode(childNodes.ElementAt(currentIndex)).Property.Name);
            Assert.AreEqual(2, childNodes.ElementAt(currentIndex).Value);
            Assert.AreEqual(0, childNodes.ElementAt(currentIndex).ChildNodes.Count());
            currentIndex++;

            Assert.AreEqual("Name", GetEdgeFromNode(childNodes.ElementAt(currentIndex)).Property.Name);
            Assert.AreEqual("World", childNodes.ElementAt(currentIndex).Value);
            Assert.AreEqual(0, childNodes.ElementAt(currentIndex).ChildNodes.Count());
            currentIndex++;

            Assert.AreEqual("createdDate", GetEdgeFromNode(childNodes.ElementAt(currentIndex)).Field.Name);
            Assert.AreEqual(new DateTime(2000, 1, 1), childNodes.ElementAt(currentIndex).Value);
            Assert.AreEqual(0, childNodes.ElementAt(currentIndex).ChildNodes.Count());
            currentIndex++;

            Assert.AreEqual("id", GetEdgeFromNode(childNodes.ElementAt(currentIndex)).Field.Name);
            Assert.AreEqual(1, childNodes.ElementAt(currentIndex).Value);
            Assert.AreEqual(0, childNodes.ElementAt(currentIndex).ChildNodes.Count());
            currentIndex++;

            Assert.AreEqual("name", GetEdgeFromNode(childNodes.ElementAt(currentIndex)).Field.Name);
            Assert.AreEqual("hello", childNodes.ElementAt(currentIndex).Value);
            Assert.AreEqual(0, childNodes.ElementAt(currentIndex).ChildNodes.Count());
            currentIndex++;
        }

        [Test]
        public void Construct_SampleObject4_DirectTwoNodeCycle()
        {
            var obj1 = new SampleObject4();
            var obj2 = new SampleObject4();
            obj1.Child = obj2;
            obj2.Child = obj1;

            var objectTree = new ObjectTree(obj1);

            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj1, rootNode.Value);

            var childNodes = objectTree.RootNode.ChildNodes.ToList();
            Assert.AreEqual(1, childNodes.Count);

            var childNode = childNodes.Single();
            Assert.AreSame(obj2, childNode.Value);

            var grandChildNodes = childNode.ChildNodes.ToList();
            Assert.AreEqual(1, grandChildNodes.Count);

            var grandChildNode = grandChildNodes.Single();
            Assert.AreSame(obj1, grandChildNode.Value);

            // Break the cycle: should have a special duplicate node, and no child nodes.
            Assert.IsInstanceOf<DuplicateObjectTreeNode>(grandChildNode);
            Assert.AreEqual(0, grandChildNode.ChildNodes.Count());
        }

        [Test]
        public void Construct_SampleObject4_DirectThreeNodeCycle()
        {
            var obj3 = new SampleObject4 { };
            var obj2 = new SampleObject4 { Child = obj3 };
            var obj1 = new SampleObject4 { Child = obj2 };
            obj3.Child = obj1;

            var objectTree = new ObjectTree(obj1);

            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj1, rootNode.Value);

            var childNode = objectTree.RootNode.ChildNodes.Single();
            Assert.AreSame(obj2, childNode.Value);

            var grandChildNode = childNode.ChildNodes.Single();
            Assert.AreSame(obj3, grandChildNode.Value);

            var greatGrandChildNode = grandChildNode.ChildNodes.Single();
            Assert.AreSame(obj1, greatGrandChildNode.Value);

            Assert.IsInstanceOf<DuplicateObjectTreeNode>(greatGrandChildNode);
            Assert.AreEqual(0, greatGrandChildNode.ChildNodes.Count());
        }

        [Test]
        public void Construct_SampleObject5And6_IndirectCycle()
        {
            var obj1 = new SampleObject5();
            var obj2 = new SampleObject6();

            obj1.Nephews = new[] { obj2 };
            obj2.Nephews = new[] { obj1 };

            var objectTree = new ObjectTree(obj1);

            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj1, rootNode.Value);

            var childListNode = rootNode.ChildNodes.Single();
            Assert.IsInstanceOf<System.Collections.IEnumerable>(childListNode.Value);

            var childElementNode = childListNode.ChildNodes.Single();
            Assert.AreSame(obj2, childElementNode.Value);

            var grandChildListNode = childElementNode.ChildNodes.Single();
            Assert.IsInstanceOf<System.Collections.IEnumerable>(grandChildListNode.Value);

            var grandChildElementNode = grandChildListNode.ChildNodes.Single();
            Assert.AreSame(obj1, grandChildElementNode.Value);

            // Break the cycle: should have a special duplicate node, and no child nodes.
            Assert.IsInstanceOf<DuplicateObjectTreeNode>(grandChildElementNode);
            Assert.AreEqual(0, grandChildElementNode.ChildNodes.Count());
        }

        [Test]
        public void Construct_SampleObject7_PropertiesWithSystemTypes()
        {
            var obj = new SampleObject7
            {
                Type = typeof(string),
                TypeToStringFunc = (Type t) => t.FullName,
                Field = typeof(string).GetField("Empty", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public),
                Property = typeof(string).GetProperty("Length"),
            };

            var objectTree = new ObjectTree(obj);

            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj, rootNode.Value);
            Assert.AreEqual(ObjectTreeNodeType.Object, rootNode.NodeType);

            var childNodes = objectTree.RootNode.ChildNodes.ToList();
            Assert.AreEqual(4, childNodes.Count);

            Assert.AreEqual("Type", GetEdgeFromNode(childNodes.ElementAt(0)).Property.Name);
            Assert.AreSame(obj.Type, childNodes.ElementAt(0).Value);
            Assert.AreEqual(0, childNodes.ElementAt(0).ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Unknown, childNodes.ElementAt(0).NodeType);

            Assert.AreEqual("TypeToStringFunc", GetEdgeFromNode(childNodes.ElementAt(1)).Property.Name);
            Assert.AreSame(obj.TypeToStringFunc, childNodes.ElementAt(1).Value);
            Assert.AreEqual(0, childNodes.ElementAt(1).ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Unknown, childNodes.ElementAt(1).NodeType);

            Assert.AreEqual("Field", GetEdgeFromNode(childNodes.ElementAt(2)).Property.Name);
            Assert.AreSame(obj.Field, childNodes.ElementAt(2).Value);
            Assert.AreEqual(0, childNodes.ElementAt(2).ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Unknown, childNodes.ElementAt(2).NodeType);

            Assert.AreEqual("Property", GetEdgeFromNode(childNodes.ElementAt(3)).Property.Name);
            Assert.AreSame(obj.Property, childNodes.ElementAt(3).Value);
            Assert.AreEqual(0, childNodes.ElementAt(3).ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Unknown, childNodes.ElementAt(3).NodeType);
        }

        [Test]
        public void Construct_AnonymousObject()
        {
            var obj = new { a = 1, b = 2 };
            var objectTree = new ObjectTree(obj);

            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj, rootNode.Value);

            Assert.AreEqual(ObjectTreeNodeType.Object, rootNode.NodeType);
            Assert.AreEqual(2, rootNode.ChildNodes.Count());

            Assert.AreEqual("a", rootNode.ChildNodes.ElementAt(0).EdgeFromParent.Member.Name);
            Assert.AreEqual(1, rootNode.ChildNodes.ElementAt(0).Value);
            Assert.AreEqual(ObjectTreeNodeType.Primitive, rootNode.ChildNodes.ElementAt(0).NodeType);
            Assert.AreEqual(0, rootNode.ChildNodes.ElementAt(0).ChildNodes.Count());

            Assert.AreEqual("b", rootNode.ChildNodes.ElementAt(1).EdgeFromParent.Member.Name);
            Assert.AreEqual(2, rootNode.ChildNodes.ElementAt(1).Value);
            Assert.AreEqual(ObjectTreeNodeType.Primitive, rootNode.ChildNodes.ElementAt(1).NodeType);
            Assert.AreEqual(0, rootNode.ChildNodes.ElementAt(1).ChildNodes.Count());
        }

        #region Test Classes

        private class SampleObject1
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public SampleChildObject1[] ChildArray { get; set; }
            public ICollection<SampleChildObject1> ChildCollection { get; set; }
        }

        private class SampleChildObject1
        {
            public int Hi { get; set; }
        }

        private class SampleObject2
        {
            public int id;
            public int Id { get; set; }

            public string name;
            public string Name { get; set; }
        }

        private class SampleObject3 : SampleObject2
        {
            public DateTime createdDate;
            public DateTime CreatedDate { get; set; }
        }

        private class SampleObject4
        {
            public SampleObject4 Child { get; set; }
        }

        private class SampleObject5
        {
            public ICollection<SampleObject6> Nephews { get; set; }
        }

        private class SampleObject6
        {
            public ICollection<SampleObject5> Nephews { get; set; }
        }

        private class SampleObject7
        {
            public Type Type { get; set; }
            public Func<Type, string> TypeToStringFunc { get; set; }
            public System.Reflection.FieldInfo Field { get; set; }
            public System.Reflection.PropertyInfo Property { get; set; }
        }

        #endregion

        #region Private

        private static ObjectTreeEdge GetEdgeFromNode(ObjectTreeNode node)
        {
            return (ObjectTreeEdge)node.EdgeFromParent;
        }

        #endregion
    }
}
