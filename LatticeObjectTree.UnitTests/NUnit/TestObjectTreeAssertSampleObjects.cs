using LatticeObjectTree.NUnit;
using LatticeObjectTree.NUnit.Constraints;
using LatticeUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree.UnitTests.NUnit
{
    public class TestObjectTreeAssertSampleObjects
    {
        [Test]
        public void EqualAddresses()
        {
            var a = CreateSampleAddress();
            var b = CreateSampleAddress();
            AssertAreEqual(a, b);
        }

        [Test]
        public void DifferentAddresses()
        {
            var a = new Address
            {
                Id = 1,
                Lines = new[] { "123 Fake ST", "Arlington, VA 22222" },
            };
            var b = new Address
            {
                Id = 1,
                Lines = new[] { "321 Fake ST", "Arlington, VA 22222" },
            };

            AssertAreNotEqual(a, b);
            AssertAreEqual(a, b, new ObjectTreeNodeFilter
            {
                ExcludedProperties = new[] { ReflectionUtils.Property<Address>(x => x.Lines) },
            });

            var assertionException = Assert.Throws<AssertionException>(() => ObjectTreeAssert.AreEqual(a, b));
            StringAssert.Contains("1 Difference", assertionException.ToString());
            StringAssert.Contains("123 Fake ST", assertionException.ToString());
            StringAssert.Contains("321 Fake ST", assertionException.ToString());
        }

        [Test]
        public void EqualCompanies()
        {
            var a = CreateSampleCompany();
            var b = CreateSampleCompany();
            AssertAreEqual(a, b);
        }

        [Test]
        public void DifferentCompanies()
        {
            var a = CreateSampleCompany();
            var b = CreateSampleCompany();
            b.Employees.First().FullName = "Robert Plant";

            Assert.AreNotEqual(a, b);

            // Try a bunch of different ways to filter out the difference so that they are equal.
            AssertAreEqual(a, b, new ObjectTreeNodeFilter
            {
                ExcludedProperties = new[] { ReflectionUtils.Property<Company>(x => x.Employees) }
            });
            AssertAreEqual(a, b, new ObjectTreeNodeFilter
            {
                ExcludedProperties = new[] { ReflectionUtils.Property<Employee>(x => x.FullName) }
            });
            AssertAreEqual(a, b, new ObjectTreeNodeFilter
            {
                ExcludedProperties = new[] { ReflectionUtils.Property<Person>(x => x.FullName) }
            });
            AssertAreEqual(a, b, new ObjectTreeNodeFilter
            {
                ExcludedPropertyNames = new[] { "FullName" }
            });
            AssertAreEqual(a, b, new ObjectTreeNodeFilter
            {
                ExcludedPropertyPredicates = new Func<PropertyInfo, bool>[] 
                { 
                    (propertyInfo) => propertyInfo.Name.Contains("Name"),
                }
            });
            AssertAreEqual(a, b, new ObjectTreeNodeFilter
            {
                ExcludedNodePredicates = new Func<ObjectTreeNode, bool>[] 
                { 
                    (node) => 
                        node.ParentNode != null && node.ParentNode.Value is Employee && ((Employee)node.ParentNode.Value).Id == 2
                        && node.EdgeFromParent != null && node.EdgeFromParent.Member.Name.Contains("Name")
                }
            });
            AssertAreNotEqual(a, b, new ObjectTreeNodeFilter
            {
                ExcludedNodePredicates = new Func<ObjectTreeNode, bool>[] 
                { 
                    (node) => 
                        node.ParentNode != null && node.ParentNode.Value is Employee && ((Employee)node.ParentNode.Value).Id == 1
                        && node.EdgeFromParent != null && node.EdgeFromParent.Member.Name.Contains("Name")
                }
            });

            var assertionException = Assert.Throws<AssertionException>(() => ObjectTreeAssert.AreEqual(a, b));
            StringAssert.Contains("1 Difference", assertionException.ToString());
            StringAssert.Contains("Robert Paulson", assertionException.ToString());
            StringAssert.Contains("Robert Plant", assertionException.ToString());
        }

        [Test]
        public void EqualPeople_Cycle()
        {
            var a = CreateSamplePerson_OwnFatherAndMotherSomehow();
            var b = CreateSamplePerson_OwnFatherAndMotherSomehow();
            AssertAreEqual(a, b);
        }

        [Test]
        public void DifferentPeople_PartialCycle()
        {
            var a = CreateSamplePerson_OwnFatherAndMotherSomehow();
            var b = CreateSamplePerson_OwnFatherAndMotherSomehow();
            b.Mother = a;
            AssertAreNotEqual(a, b);
        }

        [Test]
        public void EqualPeople_OwnGrandpa()
        {
            var a = CreateSamplePerson_OwnGrandpa();
            var b = CreateSamplePerson_OwnGrandpa();
            AssertAreEqual(a, b);
        }

        [Test]
        public void DifferentPeople_OwnGrandpa()
        {
            var a = CreateSamplePerson_OwnGrandpa();
            var b = CreateSamplePerson_OwnGrandpa();
            b.Father.FullName = "Bob";

            AssertAreNotEqual(a, b);
        }

        [Test]
        public void ListTestObjects_EmptyObjectsWithSameType()
        {
            var obj1 = new ListTestObject<string>();
            var obj2 = new ListTestObject<string>();

            AssertAreEqual(obj1, obj2);
        }

        [Test]
        public void ListTestObjects_Samples_Equal()
        {
            var obj1 = new ListTestObject<string>()
            {
                Enumerable = new[] { "test"},
                ReadOnlyCollection = new[] { "test2", "test22" },
                Collection = new[] { "test3" },
                Array = new[] { "test4" },
                IList = new[] { "test5" },
                List = new List<string> { "test6" },
            };
            var obj2 = new ListTestObject<string>()
            {
                Enumerable = new[] { "test" },
                ReadOnlyCollection = new[] { "test2", "test22" },
                Collection = new[] { "test3" },
                Array = new[] { "test4" },
                IList = new[] { "test5" },
                List = new List<string> { "test6" },
            };

            AssertAreEqual(obj1, obj2);
        }

        [Test]
        public void ListTestObjects_Samples_NotEqual()
        {
            var obj1 = new ListTestObject<string>()
            {
                Enumerable = new[] { "test" },
                ReadOnlyCollection = new[] { "test2", "test22" },
                Collection = new[] { "test3" },
                Array = new[] { "test4" },
                IList = new[] { "test5" },
                List = new List<string> { "test6" },
            };
            var obj2 = new ListTestObject<string>()
            {
                Enumerable = new[] { "TEST" },
                ReadOnlyCollection = new[] { "TEST2", "TEST22" },
                Collection = new[] { "TEST3" },
                Array = new[] { "TEST4" },
                IList = new[] { "TEST5" },
                List = new List<string> { "TEST6" },
            };

            AssertAreNotEqual(obj1, obj2);
        }

        [Test]
        public void ListTestObjects_EmptyObjectsWithDifferentTypes()
        {
            var obj1 = new ListTestObject<string>();
            var obj2 = new ListTestObject<int>();

            AssertAreNotEqual(obj1, obj2);
        }

        [Test]
        public void ListTestObjects_EmptyObjectsWithDifferentCompatibleTypes()
        {
            var obj1 = new ListTestObject<Person>();
            var obj2 = new ListTestObject<Employee>();

            AssertAreNotEqual(obj1, obj2);
        }

        #region Helpers

        private void AssertAreEqual(object expected, object actual)
        {
            ObjectTreeAssert.AreEqual(expected, actual);
            Assert.That(actual, IsObjectTree.EqualTo(expected));

            Assert.Throws<AssertionException>(() => ObjectTreeAssert.AreNotEqual(expected, actual));
            Assert.Throws<AssertionException>(() => Assert.That(actual, IsObjectTree.NotEqualTo(expected)));
            Assert.Throws<AssertionException>(() => Assert.That(actual, Is.Not.ObjectTreeEqualTo(expected)));
        }

        private void AssertAreEqual(object expected, object actual, IObjectTreeNodeFilter nodeFilter)
        {
            ObjectTreeAssert.AreEqual(expected, actual, nodeFilter);
            Assert.That(actual, IsObjectTree.EqualTo(expected).WithFilter(nodeFilter));

            Assert.Throws<AssertionException>(() => ObjectTreeAssert.AreNotEqual(expected, actual, nodeFilter));
            Assert.Throws<AssertionException>(() => Assert.That(actual, IsObjectTree.NotEqualTo(expected).WithFilter(nodeFilter)));
            Assert.Throws<AssertionException>(() => Assert.That(actual, Is.Not.ObjectTreeEqualTo(expected).WithFilter(nodeFilter)));
        }

        private void AssertAreNotEqual(object expected, object actual)
        {
            ObjectTreeAssert.AreNotEqual(expected, actual);
            Assert.That(actual, IsObjectTree.NotEqualTo(expected));
            Assert.That(actual, Is.Not.ObjectTreeEqualTo(expected));

            Assert.Throws<AssertionException>(() => ObjectTreeAssert.AreEqual(expected, actual));
            Assert.Throws<AssertionException>(() => Assert.That(actual, IsObjectTree.EqualTo(expected)));
        }

        private void AssertAreNotEqual(object expected, object actual, IObjectTreeNodeFilter nodeFilter)
        {
            ObjectTreeAssert.AreNotEqual(expected, actual, nodeFilter);
            Assert.That(actual, IsObjectTree.NotEqualTo(expected).WithFilter(nodeFilter));
            Assert.That(actual, Is.Not.ObjectTreeEqualTo(expected).WithFilter(nodeFilter));

            Assert.Throws<AssertionException>(() => ObjectTreeAssert.AreEqual(expected, actual, nodeFilter));
            Assert.Throws<AssertionException>(() => Assert.That(actual, IsObjectTree.EqualTo(expected).WithFilter(nodeFilter)));
        }

        #endregion

        #region Samples

        private Address CreateSampleAddress()
        {
            var address = new Address
            {
                Id = 1,
                Lines = new[] { "123 Fake ST", "Arlington, VA 22222" },
            };
            return address;
        }

        private Company CreateSampleCompany()
        {
            var company = new Company
            {
                Id = 1,
                Name = "The Company",
                Employees = new[]
                {
                    new Employee
                    {
                        Id = 2,
                        FullName = "Robert Paulson",
                        Addresses = new[]
                        {
                            new Address { Id = 3, AddressType = AddressType.Home, Lines = new[] { "123 Fake ST", "Arlington, VA 22222" } },
                            new Address { Id = 4, AddressType = AddressType.Work, Lines = new[] { "2 Company BLVD", "Arlington, VA 22222" } }
                        },
                        Phones = new[]
                        {
                            new Phone { Id = 5, PhoneType = PhoneType.Cell, Number = "555-555-5555", }
                        },
                    },
                    new Employee
                    {
                        Id = 6,
                        FullName = "Jenny Heath",
                        Phones = new[]
                        {
                            new Phone { Id = 7, PhoneType = PhoneType.Home, Number = "555-867-5309", }
                        },
                    },
                },
            };
            return company;
        }

        private Person CreateSamplePerson_OwnFatherAndMotherSomehow()
        {
            var person = new Person { FullName = "a" };
            person.Father = person;
            person.Mother = person;
            person.Children = new[] { person };
            return person;
        }

        private Person CreateSamplePerson_OwnGrandpa()
        {
            var protagonistFather = new Person { FullName = "Protagonist's Father" };
            var protagonistMother = new Person { FullName = "Protagonist's Mother" };
            var protagonist = new Person { FullName = "Protagonist", Father = protagonistFather, Mother = protagonistMother };

            var widow = new Person { FullName = "Widow" };
            var deadMan = new Person { FullName = "Dead Man " };
            var widowDaughter = new Person { FullName = "Widow's Daughter", Father = deadMan, Mother = widow };

            var protagonistBaby = new Person { FullName = "Protagonist's Baby", Father = protagonist, Mother = widow };
            var protagonistStepBrotherAndStepGrandchild = new Person { FullName = "Grandchild", Father = protagonistFather, Mother = widowDaughter };

            protagonist.Spouses = new[] { widow };
            widow.Spouses = new[] { deadMan, protagonist };
            deadMan.Spouses = new[] { widow };
            protagonistMother.Spouses = new[] { protagonistFather };
            protagonistFather.Spouses = new[] { protagonistMother, widowDaughter };
            widowDaughter.Spouses = new[] { protagonistFather };

            protagonist.Children = new[] { protagonistBaby };
            protagonistFather.Children = new[] { protagonist, protagonistStepBrotherAndStepGrandchild };
            protagonistMother.Children = new[] { protagonist };
            widow.Children = new[] { widowDaughter, protagonistBaby };
            widowDaughter.Children = new[] { protagonistStepBrotherAndStepGrandchild };

            return protagonist;
        }

        #endregion

        #region Test Classes

        private class Company
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<Employee> Employees { get; set; }
        }

        private class Employee : Person
        {
            public string EmployeeIdentifier { get; set; }
        }

        private class Person
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public Address HomeAddress { get { return (Addresses ?? new Address[0]).FirstOrDefault(a => a.AddressType == AddressType.Home); } }
            public ICollection<Address> Addresses { get; set; }
            public Phone HomePhone { get { return (Phones ?? new Phone[0]).FirstOrDefault(a => a.PhoneType == PhoneType.Home); } }
            public ICollection<Phone> Phones { get; set; }

            public Person Father { get; set; }
            public Person Mother { get; set; }
            public ICollection<Person> Spouses { get; set; }
            public ICollection<Person> Children { get; set; }
        }

        private class Address
        {
            public int Id { get; set;
            }
            public AddressType? AddressType { get; set; }
            public ICollection<string> Lines { get; set; }
        }

        private enum AddressType
        {
            Home, Work
        }

        private class Phone
        {
            public int Id { get; set; }
            public PhoneType? PhoneType { get; set; }
            public string Number { get; set; }
        }

        private enum PhoneType
        {
            Home, Cell, Work
        }

        private class ListTestObject<T>
        {
            public IEnumerable<T> Enumerable { get; set; } = new T[0];
            public IReadOnlyCollection<T> ReadOnlyCollection { get; set; } = new T[0];
            public ICollection<T> Collection { get; set; } = new T[0];
            public T[] Array { get; set; } = new T[0];
            public IList<T> IList { get; set; } = new T[0];
            public List<T> List { get; set; } = new List<T>();
        }

        #endregion
    }
}
