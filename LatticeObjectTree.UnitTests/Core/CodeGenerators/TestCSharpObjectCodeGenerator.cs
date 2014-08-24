using LatticeObjectTree.CodeGenerators;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree.UnitTests.Core.CodeGenerators
{
    public class TestCSharpObjectCodeGenerator
    {
        private CSharpObjectCodeGenerator codeGenerator;

        [SetUp]
        public void SetUp()
        {
            codeGenerator = new CSharpObjectCodeGenerator();
        }

        #region Primitives

        [Test]
        public void GenerateCode_Integer()
        {
            var code = codeGenerator.GenerateCode(2);
            Assert.AreEqual("2", code);
        }

        [Test]
        public void GenerateCode_Decimal()
        {
            var code = codeGenerator.GenerateCode(2.2m);
            Assert.AreEqual("2.2m", code);
        }

        [Test]
        public void GenerateCode_Float()
        {
            var code = codeGenerator.GenerateCode(2.2f);
            Assert.AreEqual("2.2f", code);
        }

        [Test]
        public void GenerateCode_Double()
        {
            var code = codeGenerator.GenerateCode(123456789.12345678);
            Assert.AreEqual("123456789.12345678d", code);
        }

        [Test]
        public void GenerateCode_String()
        {
            var code = codeGenerator.GenerateCode("Hello world!");
            Assert.AreEqual("\"Hello world!\"", code);
        }

        [Test]
        public void GenerateCode_String_EmptyString()
        {
            var code = codeGenerator.GenerateCode("");
            Assert.AreEqual("\"\"", code);
        }

        [Test]
        public void GenerateCode_Guid()
        {
            var code = codeGenerator.GenerateCode(new Guid("160dd6f7-9901-4a6e-9379-867dae92de28"));
            Assert.AreEqual("new Guid(\"160dd6f7-9901-4a6e-9379-867dae92de28\")", code);
        }

        [Test]
        public void GenerateCode_DateTime_NoTime()
        {
            var code = codeGenerator.GenerateCode(new DateTime(1970, 1, 1));
            Assert.AreEqual("new DateTime(1970, 1, 1)", code);
        }

        [Test]
        public void GenerateCode_DateTime_TimeWithoutMilliseconds()
        {
            var code = codeGenerator.GenerateCode(new DateTime(1970, 1, 1, 1, 2, 3, 0));
            Assert.AreEqual("new DateTime(1970, 1, 1, 1, 2, 3)", code);
        }

        [Test]
        public void GenerateCode_DateTime_TimeWitMilliseconds()
        {
            var code = codeGenerator.GenerateCode(new DateTime(1970, 1, 1, 1, 2, 3, 4));
            Assert.AreEqual("new DateTime(1970, 1, 1, 1, 2, 3, 4)", code);
        }

        [Test]
        public void GenerateCode_DateTimeOffset_NoTime()
        {
            var code = codeGenerator.GenerateCode(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.FromHours(-4)));
            Assert.AreEqual("new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.FromHours(-4))", code);
        }

        [Test]
        public void GenerateCode_DateTimeOffset_TimeWithoutMilliseconds()
        {
            var code = codeGenerator.GenerateCode(new DateTimeOffset(1970, 1, 1, 1, 2, 3, 0, TimeSpan.FromHours(-2)));
            Assert.AreEqual("new DateTimeOffset(1970, 1, 1, 1, 2, 3, TimeSpan.FromHours(-2))", code);
        }

        [Test]
        public void GenerateCode_DateTimeOffset_TimeWitMilliseconds()
        {
            var code = codeGenerator.GenerateCode(new DateTimeOffset(1970, 1, 1, 1, 2, 3, 4, TimeSpan.FromHours(-2)));
            Assert.AreEqual("new DateTimeOffset(1970, 1, 1, 1, 2, 3, 4, TimeSpan.FromHours(-2))", code);
        }

        #endregion

        [Test]
        public void GenerateCode_SampleObject1_AllNullValues()
        {
            var obj = new SampleObject1();
            var code = codeGenerator.GenerateCode(obj);
            Assert.AreEqual("new SampleObject1()", code);
        }

        [Test]
        public void GenerateCode_SampleObject1_OnlyPrimitiveValues()
        {
            var obj = new SampleObject1
            {
                Id = 2,
                Name = "Test",
            };
            var code = codeGenerator.GenerateCode(obj);
            Assert.AreEqual(@"new SampleObject1()
{
	Id = 2,
	Name = ""Test"",
}", code);
        }

        [Test]
        public void GenerateCode_SampleObject1_OnlyEmpyChildCollections()
        {
            var obj = new SampleObject1
            {
                ChildArray = new SampleChildObject1[0],
                ChildCollection = new List<SampleChildObject1>(),
            };
            var code = codeGenerator.GenerateCode(obj);
            Assert.AreEqual(@"new SampleObject1()
{
	ChildArray = new SampleChildObject1[0],
	ChildCollection = new List<SampleChildObject1>(),
}", code);
        }

        [Test]
        public void GenerateCode_SampleObject1_Complete()
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
            var code = codeGenerator.GenerateCode(obj);
            Assert.AreEqual(@"new SampleObject1()
{
	Id = 2,
	Name = ""Test"",
	ChildArray = new SampleChildObject1[]
	{
		new SampleChildObject1()
		{
			Hi = 3,
		},
	},
	ChildCollection = new List<SampleChildObject1>()
	{
		new SampleChildObject1()
		{
			Hi = 1,
		},
	},
}", code);
        }

        [Test]
        public void GenerateCode_SampleObject4_DirectOneNodeCycle()
        {
            var obj1 = new SampleObject4();
            obj1.Child = obj1;

            var code = codeGenerator.GenerateCode(obj1);
            Assert.AreEqual(@"new SampleObject4()
{
	Child = <root>,
}", code);
        }

        [Test]
        public void GenerateCode_SampleObject4_DirectOneNodeCycle_RootVariableName()
        {
            var obj1 = new SampleObject4();
            obj1.Child = obj1;

            codeGenerator = new CSharpObjectCodeGenerator(new CSharpObjectCodeGeneratorOptions { RootVariableName = "test" });
            var code = codeGenerator.GenerateCode(obj1);
            Assert.AreEqual(@"var test = new SampleObject4()
{
	Child = test,
}", code);
        }

        [Test]
        public void GenerateCode_SampleObject4_DirectThreeNodeCycle()
        {
            var obj3 = new SampleObject4 { };
            var obj2 = new SampleObject4 { Child = obj3 };
            var obj1 = new SampleObject4 { Child = obj2 };
            obj3.Child = obj1;

            var code = codeGenerator.GenerateCode(obj1);
            Assert.AreEqual(@"new SampleObject4()
{
	Child = new SampleObject4()
	{
		Child = new SampleObject4()
		{
			Child = <root>,
		},
	},
}", code);

        }

        [Test]
        public void GenerateCode_SampleObject4_DirectThreeNodeCycle_RootVariableName()
        {
            var obj3 = new SampleObject4 { };
            var obj2 = new SampleObject4 { Child = obj3 };
            var obj1 = new SampleObject4 { Child = obj2 };
            obj3.Child = obj1;

            codeGenerator = new CSharpObjectCodeGenerator(new CSharpObjectCodeGeneratorOptions { RootVariableName = "test" });
            var code = codeGenerator.GenerateCode(obj1);
            Assert.AreEqual(@"var test = new SampleObject4()
{
	Child = new SampleObject4()
	{
		Child = new SampleObject4()
		{
			Child = test,
		},
	},
}", code);

        }

        [Test]
        public void GenerateCode_SampleObject4_DirectThreeNodeCycleAlt1_RootVariableName()
        {
            var obj3 = new SampleObject4 { };
            var obj2 = new SampleObject4 { Child = obj3 };
            var obj1 = new SampleObject4 { Child = obj2 };
            obj3.Child = obj2;

            codeGenerator = new CSharpObjectCodeGenerator(new CSharpObjectCodeGeneratorOptions { RootVariableName = "test" });
            var code = codeGenerator.GenerateCode(obj1);
            Assert.AreEqual(@"var test = new SampleObject4()
{
	Child = new SampleObject4()
	{
		Child = new SampleObject4()
		{
			Child = test.Child,
		},
	},
}", code);

        }

        [Test]
        public void GenerateCode_SampleObject4_DirectThreeNodeCycleAlt2_RootVariableName()
        {
            var obj3 = new SampleObject4 { };
            var obj2 = new SampleObject4 { Child = obj3 };
            var obj1 = new SampleObject4 { Child = obj2 };
            obj3.Child = obj3;

            codeGenerator = new CSharpObjectCodeGenerator(new CSharpObjectCodeGeneratorOptions { RootVariableName = "test" });
            var code = codeGenerator.GenerateCode(obj1);
            Assert.AreEqual(@"var test = new SampleObject4()
{
	Child = new SampleObject4()
	{
		Child = new SampleObject4()
		{
			Child = test.Child.Child,
		},
	},
}", code);

        }

        [Test]
        public void GenerateCode_SampleObject5()
        {
            var obj = new SampleObject5(1, 2);
            var code = codeGenerator.GenerateCode(obj);
            Assert.AreEqual(@"new SampleObject5(test1: 1, test2: 2)
{
	Test2 = 2,
}", code);
        }

        [Test]
        public void GenerateCode_SampleObject6()
        {
            var obj = new SampleObject6(1, 2, "test", 3, 4);
            var code = codeGenerator.GenerateCode(obj);
            Assert.AreEqual(@"new SampleObject6(test3: default(int), test1: 2, test4: default(string), test2: 3, TEST2: 4)
{
	Test2 = 3,
	TEST2 = 4,
}", code);
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

        public class SampleObject5
        {
            public SampleObject5(int test1, int test2) 
            {
                Test1 = test1;
                Test2 = test2;
            }
            public int Test1 { get; private set; }
            public int Test2 { get; set; }
        }

        public class SampleObject6
        {
            public SampleObject6(int test3, int test1, string test4, int test2, int TEST2) 
            {
                this.test1 = test1;
                this.Test2 = test2;
                this.TEST2 = TEST2;
            }

            public int Test1 { get { return test1; } }
            private int test1;

            public int Test2 { get; set; }
            public int TEST2 { get; set; }
        }

        #endregion
    }

}
