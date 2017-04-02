using LatticeObjectTree.Exceptions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LatticeObjectTree
{
    public class ObjectTreeAssertTest
    {
        [Test]
        public void AreEqual_CompareObjectToItself()
        {
            var obj = new object();
            ObjectTreeAssert.AreEqual(obj, obj);
        }

        [Test]
        public void AreNotEqual_TwoDifferentObjects()
        {
            ObjectTreeAssert.AreNotEqual(new object(), new object());
        }

        #region Messages

        [Test]
        public void AreEqualFail_DifferentStrings()
        {
            var expected = "hello";
            var actual = "world";
            var expectedException = Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(expected, actual));

            var expectedMessage = "ObjectTreeAssert.AreEqual() Failure" + Environment.NewLine
                + "1 Difference:" + Environment.NewLine
                + "\t<root>: expected value \"hello\" but was \"world\".";
            AssertAreEqualMultilineStrings(expectedMessage, expectedException.Message);
        }

        [Test]
        public void AreEqualFail_DifferentAnonymousObjects()
        {
            var expected = new { id = 1, message = "hello" };
            var actual = new { id = 2, message = "world" };
            var expectedException = Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(expected, actual));

            var expectedMessage = "ObjectTreeAssert.AreEqual() Failure" + Environment.NewLine
                + "2 Differences:" + Environment.NewLine
                + "\t<root>.id: expected value 1 but was 2." + Environment.NewLine
                + "\t<root>.message: expected value \"hello\" but was \"world\".";
            Assert.AreEqual(expectedMessage, expectedException.Message);
        }

        [Test]
        public void AreEqualFail_DifferentCustomObjects()
        {
            var expected = new Company { Id = 1, Name = "hello" };
            var actual = new Company { Id = 2, Name = "world" };
            var expectedException = Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(expected, actual));

            var expectedMessage = "ObjectTreeAssert.AreEqual() Failure" + Environment.NewLine
                + "2 Differences:" + Environment.NewLine
                + "\t<root>.Id: expected value 1 but was 2." + Environment.NewLine
                + "\t<root>.Name: expected value \"hello\" but was \"world\".";

            Assert.AreEqual(expectedMessage, expectedException.Message);
        }

        [Test]
        public void AreEqualFail_DifferentLists()
        {
            var expected = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            var actual = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 };
            var expectedException = Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(expected, actual));

            var expectedMessage = "ObjectTreeAssert.AreEqual() Failure" + Environment.NewLine
                 + "26 Differences:" + Environment.NewLine
                 + "	<root>[0]: expected value 0 but was 1." + Environment.NewLine
                 + "	<root>[1]: expected value 1 but was 2." + Environment.NewLine
                 + "	<root>[2]: expected value 2 but was 3." + Environment.NewLine
                 + "	<root>[3]: expected value 3 but was 4." + Environment.NewLine
                 + "	<root>[4]: expected value 4 but was 5." + Environment.NewLine
                 + "	<root>[5]: expected value 5 but was 6." + Environment.NewLine
                 + "	<root>[6]: expected value 6 but was 7." + Environment.NewLine
                 + "	<root>[7]: expected value 7 but was 8." + Environment.NewLine
                 + "	<root>[8]: expected value 8 but was 9." + Environment.NewLine
                 + "	<root>[9]: expected value 9 but was 10." + Environment.NewLine
                 + "	<root>[10]: expected value 10 but was 11." + Environment.NewLine
                 + "	<root>[11]: expected value 11 but was 12." + Environment.NewLine
                 + "	<root>[12]: expected value 12 but was 13." + Environment.NewLine
                 + "	<root>[13]: expected value 13 but was 14." + Environment.NewLine
                 + "	<root>[14]: expected value 14 but was 15." + Environment.NewLine
                 + "	<root>[15]: expected value 15 but was 16." + Environment.NewLine
                 + "	<root>[16]: expected value 16 but was 17." + Environment.NewLine
                 + "	<root>[17]: expected value 17 but was 18." + Environment.NewLine
                 + "	<root>[18]: expected value 18 but was 19." + Environment.NewLine
                 + "	<root>[19]: expected value 19 but was 20." + Environment.NewLine
                 + "	<root>[20]: expected value 20 but was 21." + Environment.NewLine
                 + "	<root>[21]: expected value 21 but was 22." + Environment.NewLine
                 + "	<root>[22]: expected value 22 but was 23." + Environment.NewLine
                 + "	<root>[23]: expected value 23 but was 24." + Environment.NewLine
                 + "	<root>[24]: expected value 24 but was 25." + Environment.NewLine
                 + "	<root>[25]: expected value 25 but was 26.";
            Assert.AreEqual(expectedMessage, expectedException.Message);
        }

        [Test]
        public void AreNotEqualFail_SameString()
        {
            var expected = "hello";
            var actual = "hello";
            var expectedException = Assert.Throws<ObjectTreeNotEqualException>(() => ObjectTreeAssert.AreNotEqual(expected, actual));

            var expectedMessage = "ObjectTreeAssert.AreNotEqual() Failure";
            Assert.AreEqual(expectedMessage, expectedException.Message);
        }

        [Test]
        public void AreEqualFail_VeryLongValues()
        {
            var obj1 = new Person
            {
                FullName = string.Join(string.Empty, Enumerable.Range(0, 512)),
            };
            var obj2 = new Person
            {
                FullName = string.Join(string.Empty, Enumerable.Range(1, 512)),
            };

            var assertionException = Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(obj1, obj2));
            var expectedMessage = @"ObjectTreeAssert.AreEqual() Failure
1 Difference:
	<root>.FullName: expected value ""0123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737…"" but was ""1234567891011121314151617181920212223242526272829303132333435363738394041424344454647484950515253545556575859606162636465666768697071727374…"".";
            AssertAreEqualMultilineStrings(expectedMessage, assertionException.Message);
        }

        [Test]
        public void AreEqualFail_LotsOfDifferences()
        {
            var obj1 = new ListTestObject<int>
            {
                List = Enumerable.Range(0, 512).ToList(),
            };
            var obj2 = new ListTestObject<int>
            {
                List = Enumerable.Range(1, 512).ToList(),
            };

            var assertionException = Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(obj1, obj2));
            var expectedMessage = @"ObjectTreeAssert.AreEqual() Failure
99+ Differences:
	<root>.List[0]: expected value 0 but was 1.
	<root>.List[1]: expected value 1 but was 2.
	<root>.List[2]: expected value 2 but was 3.
	<root>.List[3]: expected value 3 but was 4.
	<root>.List[4]: expected value 4 but was 5.
	<root>.List[5]: expected value 5 but was 6.
	<root>.List[6]: expected value 6 but was 7.
	<root>.List[7]: expected value 7 but was 8.
	<root>.List[8]: expected value 8 but was 9.
	<root>.List[9]: expected value 9 but was 10.
	<root>.List[10]: expected value 10 but was 11.
	<root>.List[11]: expected value 11 but was 12.
	<root>.List[12]: expected value 12 but was 13.
	<root>.List[13]: expected value 13 but was 14.
	<root>.List[14]: expected value 14 but was 15.
	<root>.List[15]: expected value 15 but was 16.
	<root>.List[16]: expected value 16 but was 17.
	<root>.List[17]: expected value 17 but was 18.
	<root>.List[18]: expected value 18 but was 19.
	<root>.List[19]: expected value 19 but was 20.
	<root>.List[20]: expected value 20 but was 21.
	<root>.List[21]: expected value 21 but was 22.
	<root>.List[22]: expected value 22 but was 23.
	<root>.List[23]: expected value 23 but was 24.
	<root>.List[24]: expected value 24 but was 25.
	<root>.List[25]: expected value 25 but was 26.
	<root>.List[26]: expected value 26 but was 27.
	<root>.List[27]: expected value 27 but was 28.
	<root>.List[28]: expected value 28 but was 29.
	<root>.List[29]: expected value 29 but was 30.
	<root>.List[30]: expected value 30 but was 31.
	<root>.List[31]: expected value 31 but was 32.
	<root>.List[32]: expected value 32 but was 33.
	<root>.List[33]: expected value 33 but was 34.
	<root>.List[34]: expected value 34 but was 35.
	<root>.List[35]: expected value 35 but was 36.
	<root>.List[36]: expected value 36 but was 37.
	<root>.List[37]: expected value 37 but was 38.
	<root>.List[38]: expected value 38 but was 39.
	<root>.List[39]: expected value 39 but was 40.
	<root>.List[40]: expected value 40 but was 41.
	<root>.List[41]: expected value 41 but was 42.
	<root>.List[42]: expected value 42 but was 43.
	<root>.List[43]: expected value 43 but was 44.
	<root>.List[44]: expected value 44 but was 45.
	<root>.List[45]: expected value 45 but was 46.
	<root>.List[46]: expected value 46 but was 47.
	<root>.List[47]: expected value 47 but was 48.
	<root>.List[48]: expected value 48 but was 49.
	<root>.List[49]: expected value 49 but was 50.
	<root>.List[50]: expected value 50 but was 51.
	<root>.List[51]: expected value 51 but was 52.
	<root>.List[52]: expected value 52 but was 53.
	<root>.List[53]: expected value 53 but was 54.
	<root>.List[54]: expected value 54 but was 55.
	<root>.List[55]: expected value 55 but was 56.
	<root>.List[56]: expected value 56 but was 57.
	<root>.List[57]: expected value 57 but was 58.
	<root>.List[58]: expected value 58 but was 59.
	<root>.List[59]: expected value 59 but was 60.
	<root>.List[60]: expected value 60 but was 61.
	<root>.List[61]: expected value 61 but was 62.
	<root>.List[62]: expected value 62 but was 63.
	<root>.List[63]: expected value 63 but was 64.
	<root>.List[64]: expected value 64 but was 65.
	<root>.List[65]: expected value 65 but was 66.
	<root>.List[66]: expected value 66 but was 67.
	<root>.List[67]: expected value 67 but was 68.
	<root>.List[68]: expected value 68 but was 69.
	<root>.List[69]: expected value 69 but was 70.
	<root>.List[70]: expected value 70 but was 71.
	<root>.List[71]: expected value 71 but was 72.
	<root>.List[72]: expected value 72 but was 73.
	<root>.List[73]: expected value 73 but was 74.
	<root>.List[74]: expected value 74 but was 75.
	<root>.List[75]: expected value 75 but was 76.
	<root>.List[76]: expected value 76 but was 77.
	<root>.List[77]: expected value 77 but was 78.
	<root>.List[78]: expected value 78 but was 79.
	<root>.List[79]: expected value 79 but was 80.
	<root>.List[80]: expected value 80 but was 81.
	<root>.List[81]: expected value 81 but was 82.
	<root>.List[82]: expected value 82 but was 83.
	<root>.List[83]: expected value 83 but was 84.
	<root>.List[84]: expected value 84 but was 85.
	<root>.List[85]: expected value 85 but was 86.
	<root>.List[86]: expected value 86 but was 87.
	<root>.List[87]: expected value 87 but was 88.
	<root>.List[88]: expected value 88 but was 89.
	<root>.List[89]: expected value 89 but was 90.
	<root>.List[90]: expected value 90 but was 91.
	<root>.List[91]: expected value 91 but was 92.
	<root>.List[92]: expected value 92 but was 93.
	<root>.List[93]: expected value 93 but was 94.
	<root>.List[94]: expected value 94 but was 95.
	<root>.List[95]: expected value 95 but was 96.
	<root>.List[96]: expected value 96 but was 97.
	<root>.List[97]: expected value 97 but was 98.
	<root>.List[98]: expected value 98 but was 99.";
            Assert.AreEqual(expectedMessage, assertionException.Message);
        }

        #endregion

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
                ExcludedProperties = new[] { typeof(Address).GetProperty(nameof(Address.Lines)) },
            });

            var assertionException = Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(a, b));
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
                ExcludedProperties = new[] { typeof(Company).GetProperty(nameof(Company.Employees)) }
            });
            AssertAreEqual(a, b, new ObjectTreeNodeFilter
            {
                ExcludedProperties = new[] { typeof(Employee).GetProperty(nameof(Employee.FullName)) }
            });
            AssertAreEqual(a, b, new ObjectTreeNodeFilter
            {
                ExcludedProperties = new[] { typeof(Person).GetProperty(nameof(Person.FullName)) }
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

            var assertionException = Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(a, b));
            StringAssert.Contains("1 Difference", assertionException.ToString());
            StringAssert.Contains("Robert Paulson", assertionException.ToString());
            StringAssert.Contains("Robert Plant", assertionException.ToString());
        }

        [Test]
        public void DifferentCompanies_PrivateGetters()
        {
            var a = new CompanyPrivateGetters
            {
                Id = 1,
                Name = "The Company",
            };
            var b = new CompanyPrivateGetters
            {
                Id = 1,
                Name = "A Company",
            };

            // I guess private getters on a public property won't stop us?
            Assert.AreNotEqual(a, b);

            var assertionException = Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(a, b));
            StringAssert.Contains("1 Difference", assertionException.ToString());
            StringAssert.Contains("The Company", assertionException.ToString());
            StringAssert.Contains("A Company", assertionException.ToString());
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
                Enumerable = new[] { "test" },
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

        [Test]
        public void ReflectionPropertiesObject_Sample()
        {
            var obj1 = new ReflectionPropertiesObject()
            {
                Type = typeof(string),
                PropertyInfo = typeof(string).GetProperty(nameof(string.Length)),
            };
            var obj2 = new ReflectionPropertiesObject()
            {
                Type = typeof(ReflectionPropertiesObject),
                PropertyInfo = typeof(ReflectionPropertiesObject).GetProperty(nameof(ReflectionPropertiesObject.PropertyInfo)),
            };
            AssertAreNotEqual(obj1, obj2);
        }

        [Test]
        public void EmptyArrays()
        {
            var obj1 = new string[0];
            var obj2 = new string[0];
            AssertAreEqual(obj1, obj2);
        }

        #region Helpers

        private void AssertAreEqual(object expected, object actual)
        {
            ObjectTreeAssert.AreEqual(expected, actual);
            Assert.Throws<ObjectTreeNotEqualException>(() => ObjectTreeAssert.AreNotEqual(expected, actual));
        }

        private void AssertAreEqual(object expected, object actual, IObjectTreeNodeFilter nodeFilter)
        {
            ObjectTreeAssert.AreEqual(expected, actual, nodeFilter);
            Assert.Throws<ObjectTreeNotEqualException>(() => ObjectTreeAssert.AreNotEqual(expected, actual, nodeFilter));
        }

        private void AssertAreNotEqual(object expected, object actual)
        {
            ObjectTreeAssert.AreNotEqual(expected, actual);
            Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(expected, actual));
        }

        private void AssertAreNotEqual(object expected, object actual, IObjectTreeNodeFilter nodeFilter)
        {
            ObjectTreeAssert.AreNotEqual(expected, actual, nodeFilter);
            Assert.Throws<ObjectTreeEqualException>(() => ObjectTreeAssert.AreEqual(expected, actual, nodeFilter));
        }

        private static void AssertAreEqualMultilineStrings(string expected, string actual)
        {
            Assert.AreEqual(NormalizeLineEndings(expected), NormalizeLineEndings(actual));
        }

        private static string NormalizeLineEndings(string input)
        {
            if (input == null) return null;
            return System.Text.RegularExpressions.Regex.Replace(input, @"\r\n|\n|\r", "\n");
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

        private class CompanyPrivateGetters
        {
            public int Id { private get; set; }
            public string Name { private get; set; }
        }

        private class Employee : Person
        {
            public string EmployeeIdentifier { get; set; }
        }

        private class Person
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public Address HomeAddress => Addresses?.FirstOrDefault(a => a.AddressType == AddressType.Home);
            public ICollection<Address> Addresses { get; set; }
            public Phone HomePhone => Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Home);
            public ICollection<Phone> Phones { get; set; }

            public Person Father { get; set; }
            public Person Mother { get; set; }
            public ICollection<Person> Spouses { get; set; }
            public ICollection<Person> Children { get; set; }
        }

        private class Address
        {
            public int Id
            {
                get; set;
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

        private class ReflectionPropertiesObject
        {
            public Type Type { get; set; }
            public Type TypeGetOnly => Type;

            public PropertyInfo PropertyInfo { get; set; }
            public PropertyInfo PropertyInfoGetOnly => PropertyInfo;
        }

        #endregion
    }
}
