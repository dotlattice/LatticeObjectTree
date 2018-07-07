using NUnit.Framework;
using TestAttribute = Xunit.FactAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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
                Field = typeof(string).GetRuntimeField("Empty"),
                Property = typeof(string).GetRuntimeProperty("Length"),
                Task = Task.CompletedTask,
                CancellationToken = CancellationToken.None,
                CancellationTokenSource = new CancellationTokenSource(),
            };

            var objectTree = new ObjectTree(obj);

            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj, rootNode.Value);
            Assert.AreEqual(ObjectTreeNodeType.Object, rootNode.NodeType);

            var childNodes = objectTree.RootNode.ChildNodes.ToList();
            Assert.AreEqual(7, childNodes.Count);

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

            Assert.AreEqual("Task", GetEdgeFromNode(childNodes.ElementAt(4)).Property.Name);
            Assert.AreSame(obj.Task, childNodes.ElementAt(4).Value);
            Assert.AreEqual(0, childNodes.ElementAt(4).ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Unknown, childNodes.ElementAt(4).NodeType);

            Assert.AreEqual("CancellationToken", GetEdgeFromNode(childNodes.ElementAt(5)).Property.Name);
            Assert.AreEqual(obj.CancellationToken, childNodes.ElementAt(5).Value);
            Assert.AreEqual(0, childNodes.ElementAt(5).ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Unknown, childNodes.ElementAt(5).NodeType);

            Assert.AreEqual("CancellationTokenSource", GetEdgeFromNode(childNodes.ElementAt(6)).Property.Name);
            Assert.AreEqual(obj.CancellationTokenSource, childNodes.ElementAt(6).Value);
            Assert.AreEqual(0, childNodes.ElementAt(6).ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Unknown, childNodes.ElementAt(6).NodeType);
        }

        [Test]
        public void Construct_SampleObject8_PublicConstField()
        {
            var obj = new SampleObject8();

            var objectTree = new ObjectTree(obj);

            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj, rootNode.Value);
            Assert.AreEqual(ObjectTreeNodeType.Object, rootNode.NodeType);

            var childNodes = objectTree.RootNode.ChildNodes.ToList();
            Assert.AreEqual(0, childNodes.Count);
        }

        [Test]
        public void Construct_SampleObject9_PrimitiveValues()
        {
            var obj = SampleObject9.CreateSample();

            var objectTree = new ObjectTree(obj);

            var rootNode = objectTree.RootNode;
            Assert.AreSame(obj, rootNode.Value);
            Assert.AreEqual(ObjectTreeNodeType.Object, rootNode.NodeType);

            var childNodes = objectTree.RootNode.ChildNodes.ToList();
            Assert.AreEqual(34, childNodes.Count);
            for (var i = 0; i < childNodes.Count; i++)
            {
                Assert.AreEqual(ObjectTreeNodeType.Primitive, childNodes[i].NodeType);
            }
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

        [Test]
        public void Construct_SampleStruct1()
        {
            var obj = new SampleStruct1
            {
                IntValue = 2,
                SampleStruct2 = new SampleStruct2
                {
                    StringValue = "two",
                },
            };

            var objectTree = new ObjectTree(obj);

            var rootNode = objectTree.RootNode;
            Assert.AreEqual(ObjectTreeNodeType.Object, rootNode.NodeType);

            var childNodes = objectTree.RootNode.ChildNodes.ToList();
            Assert.AreEqual(2, childNodes.Count);

            Assert.AreEqual("IntValue", GetEdgeFromNode(childNodes.ElementAt(0)).Property.Name);
            Assert.AreEqual(obj.IntValue, childNodes.ElementAt(0).Value);
            Assert.AreEqual(0, childNodes.ElementAt(0).ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Primitive, childNodes.ElementAt(0).NodeType);

            Assert.AreEqual("SampleStruct2", GetEdgeFromNode(childNodes.ElementAt(1)).Property.Name);
            Assert.AreEqual(1, childNodes.ElementAt(1).ChildNodes.Count());
            Assert.AreEqual(ObjectTreeNodeType.Object, childNodes.ElementAt(1).NodeType);
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
            public Task Task { get; set; }
            public CancellationToken CancellationToken { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }

        private class SampleObject8
        {
            public const int ConstIntValue = 2;
        }

        private class SampleObject9
        {
            public static SampleObject9 CreateSample()
            {
                return new SampleObject9
                {
                    StringValue = "two",
                    IntValue = 1,
                    NullableIntValue = 2,
                    UIntValue = 3,
                    NullableUIntValue = 4,
                    LongValue = 5L,
                    NullableLongValue = 6L,
                    ULongValue = 7L,
                    NullableULongValue = 8L,
                    ShortValue = 9,
                    NullableShortValue = 10,
                    UShortValue = 11,
                    NullableUShortValue = 12,
                    ByteValue = 13,
                    NullableByteValue = 14,
                    SByteValue = 15,
                    NullableSByteValue = 16,
                    BoolValue = true,
                    NullableBoolValue = false,
                    DecimalValue = 1.1m,
                    NullableDecimalValue = 2.2m,
                    DoubleValue = 3.3d,
                    NullableDoubleValue = 4.4d,
                    FloatValue = 5.5d,
                    NullableFloatValue = 6.6d,
                    DateTimeValue = new DateTime(2000, 1, 1, 12, 1, 2),
                    NullableDateTimeValue = new DateTime(2000, 1, 2, 12, 1, 2),
                    DateTimeOffsetValue = new DateTimeOffset(2000, 1, 3, 12, 1, 2, TimeSpan.Zero),
                    NullableDateTimeOffsetValue = new DateTimeOffset(2000, 1, 4, 12, 1, 2, TimeSpan.Zero),
                    TimeSpanValue = TimeSpan.FromHours(1),
                    NullableTimeSpanValue = TimeSpan.FromHours(2),
                    GuidValue = new Guid("e9e13594-6cb4-45f0-b20b-b4e947161256"),
                    NullableGuidValue = new Guid("36408fcb-eb6c-4440-95ed-39ec43866347"),
                    ByteArrayValue = new byte[] { 1, 2, 255 },
                };
            }

            public string StringValue { get; set; }
            public int IntValue { get; set; }
            public int? NullableIntValue { get; set; }
            public uint UIntValue { get; set; }
            public uint? NullableUIntValue { get; set; }
            public long LongValue { get; set; }
            public long? NullableLongValue { get; set; }
            public ulong ULongValue { get; set; }
            public ulong? NullableULongValue { get; set; }
            public short ShortValue { get; set; }
            public short? NullableShortValue { get; set; }
            public ushort UShortValue { get; set; }
            public ushort? NullableUShortValue { get; set; }
            public byte ByteValue { get; set; }
            public byte? NullableByteValue { get; set; }
            public sbyte SByteValue { get; set; }
            public sbyte? NullableSByteValue { get; set; }
            public bool BoolValue { get; set; }
            public bool? NullableBoolValue { get; set; }
            public decimal DecimalValue { get; set; }
            public decimal? NullableDecimalValue { get; set; }
            public double DoubleValue { get; set; }
            public double? NullableDoubleValue { get; set; }
            public double FloatValue { get; set; }
            public double? NullableFloatValue { get; set; }
            public DateTime DateTimeValue { get; set; }
            public DateTime? NullableDateTimeValue { get; set; }
            public DateTimeOffset DateTimeOffsetValue { get; set; }
            public DateTimeOffset? NullableDateTimeOffsetValue { get; set; }
            public TimeSpan TimeSpanValue { get; set; }
            public TimeSpan? NullableTimeSpanValue { get; set; }
            public Guid GuidValue { get; set; }
            public Guid? NullableGuidValue { get; set; }
            public byte[] ByteArrayValue { get; set; }
        }


        private struct SampleStruct1
        {
            public int IntValue { get; set; }
            public SampleStruct2 SampleStruct2 { get; set; }
        }

        private struct SampleStruct2
        {
            public string StringValue { get; set; }
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
