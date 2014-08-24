using LatticeObjectTree.Comparers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree.UnitTests.Core.Comparers
{
    public class TestObjectTreeEqualityComparer
    {
        private ObjectTreeEqualityComparer objectComparer;

        [SetUp]
        public void SetUp()
        {
            objectComparer = new ObjectTreeEqualityComparer();
        }

        [TestCase(2, 2, 0)]
        [TestCase(1, 2, 1)]
        [TestCase(2.0f, 2.0f, 0)]
        [TestCase(1.0f, 2.0f, 1)]
        [TestCase("hello", "hello", 0)]
        [TestCase("hello", "world", 1)]
        public void FindDifferences_SimpleValue(object a, object b, int expectedDifferenceCount)
        {
            var differences = objectComparer.FindDifferences(a, b).ToList();
            Assert.AreEqual(expectedDifferenceCount, differences.Count);
        }

        [Test]
        public void FindDifferences_PlainObjects()
        {
            var a = new object();
            var b = new object();
            var differences = objectComparer.FindDifferences(a, b).ToList();
            Assert.AreEqual(1, differences.Count);

            var difference = differences.Single();
            Assert.AreSame(a, difference.Expected.Value);
            Assert.AreSame(b, difference.Actual.Value);
        }

        [Test]
        public void FindDifferences_SampleObject1_SameObject()
        {
            var a = new SampleObject1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new[] 
                { 
                    new SampleChildObject1 { Hi = 1, },
                    new SampleChildObject1 { Hi = 2, },
                },
                ChildCollection = new[] 
                { 
                    new SampleChildObject1 { Hi = 3, },
                    new SampleChildObject1 { Hi = 4, },
                }
            };

            var differences = objectComparer.FindDifferences(a, a).ToList();
            Assert.AreEqual(0, differences.Count);
        }

        [Test]
        public void FindDifferences_SampleObject1_Equal()
        {
            var a = new SampleObject1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new[] 
                { 
                    new SampleChildObject1 { Hi = 1, },
                    new SampleChildObject1 { Hi = 2, },
                },
                ChildCollection = new[] 
                { 
                    new SampleChildObject1 { Hi = 3, },
                    new SampleChildObject1 { Hi = 4, },
                }
            };
            var b = new SampleObject1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new[] 
                { 
                    new SampleChildObject1 { Hi = 1, },
                    new SampleChildObject1 { Hi = 2, },
                },
                ChildCollection = new[] 
                { 
                    new SampleChildObject1 { Hi = 3, },
                    new SampleChildObject1 { Hi = 4, },
                }
            };
            var differences = objectComparer.FindDifferences(a, b).ToList();
            Assert.AreEqual(0, differences.Count);
        }

        [Test]
        public void FindDifferences_SampleObject1_DifferentValues()
        {
            var a = new SampleObject1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new[] 
                { 
                    new SampleChildObject1 { Hi = 1, },
                    new SampleChildObject1 { Hi = 2, },
                },
                ChildCollection = new[] 
                { 
                    new SampleChildObject1 { Hi = 3, },
                    new SampleChildObject1 { Hi = 4, },
                }
            };
            var b = new SampleObject1
            {
                Id = 1,
                Name = "Test2",
                ChildArray = new[] 
                { 
                    new SampleChildObject1 { Hi = 1, },
                    new SampleChildObject1 { Hi = 2, },
                },
                ChildCollection = new[] 
                { 
                    new SampleChildObject1 { Hi = 22, },
                    new SampleChildObject1 { Hi = 4, },
                }
            };
            var differences = objectComparer.FindDifferences(a, b).ToList();
            Assert.AreEqual(2, differences.Count);

            {
                var diff = differences.ElementAt(0);
                StringAssert.Contains("\"Test\"", diff.ToString());
                StringAssert.Contains("\"Test2\"", diff.ToString());
            }
            {
                var diff = differences.ElementAt(1);
                StringAssert.Contains("\"3\"", diff.ToString());
                StringAssert.Contains("\"22\"", diff.ToString());
            }
        }

        [Test]
        public void FindDifferences_SampleObject1_DifferentCollectionLengths()
        {
            var a = new SampleObject1
            {
                Id = 1,
                Name = "Test",
                ChildArray = new[] 
                { 
                    new SampleChildObject1 { Hi = 1, },
                    new SampleChildObject1 { Hi = 2, },
                },
                ChildCollection = new[] 
                { 
                    new SampleChildObject1 { Hi = 3, },
                    new SampleChildObject1 { Hi = 4, },
                }
            };
            var b = new SampleObject1
            {
                Id = 1,
                Name = "Test",
                ChildCollection = new[] 
                { 
                    new SampleChildObject1 { Hi = 3, },
                    new SampleChildObject1 { Hi = 4, },
                    new SampleChildObject1 { Hi = 5, },
                }
            };
            var differences = objectComparer.FindDifferences(a, b).ToList();
            Assert.AreEqual(4, differences.Count);

            {
                var diff = differences.ElementAt(0);
                StringAssert.Contains("expected 2 children", diff.ToString());
                StringAssert.Contains("had 0 children", diff.ToString());
            }
            {
                var diff = differences.ElementAt(1);
                StringAssert.Contains("a child", diff.ToString());
            }
        }

        [Test]
        public void FindDifferences_SampleObject4_DirectOneNodeCycle_DifferentRootObjects()
        {
            var obj1 = new SampleObject4();
            obj1.Child = obj1;

            var obj2 = new SampleObject4();
            obj2.Child = obj1;

            var differences = objectComparer.FindDifferences(obj1, obj2).ToList();
            Assert.AreEqual(1, differences.Count);


        }

        [Test]
        public void FindDifferences_SampleObject4_DirectOneNodeCycle_SameRootObject()
        {
            var obj1 = new SampleObject4();
            obj1.Child = obj1;

            var differences = objectComparer.FindDifferences(obj1, obj1).ToList();
            Assert.AreEqual(0, differences.Count);
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

        private class SampleObject4
        {
            public SampleObject4 Child { get; set; }
        }

        #endregion
    }
}
