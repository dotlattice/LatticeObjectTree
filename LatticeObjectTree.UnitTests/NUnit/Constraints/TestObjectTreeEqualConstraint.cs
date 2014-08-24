using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using LatticeObjectTree.NUnit.Constraints;

namespace LatticeObjectTree.UnitTests.NUnit.Constraints
{
    public class TestObjectTreeEqualConstraint
    {
        [TestCase(true)]
        [TestCase(false)]
        public void WriteActualValueTo_StringValue(bool isMatch)
        {
            var expectedValue = "hello";
            var actualValue = isMatch ? expectedValue : "world";
            var constraint = new ObjectTreeEqualConstraint(expectedValue);
            Assert.AreEqual(isMatch, constraint.Matches(actualValue));

            var writer = new TextMessageWriter();
            constraint.WriteActualValueTo(writer);

            var expectedMessage = string.Format("<LatticeObjectTree.ObjectTree> with root \"{0}\"", actualValue);
            Assert.AreEqual(expectedMessage, writer.ToString());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WriteDescriptionTo_StringValue(bool isMatch)
        {
            var expectedValue = "hello";
            var actualValue = isMatch ? expectedValue : "world";
            var constraint = new ObjectTreeEqualConstraint(expectedValue);
            Assert.AreEqual(isMatch, constraint.Matches(actualValue));

            var writer = new TextMessageWriter();
            constraint.WriteDescriptionTo(writer);

            var expectedMessage = string.Format("<LatticeObjectTree.ObjectTree> with root \"{0}\"", expectedValue);
            Assert.AreEqual(expectedMessage, writer.ToString());
        }

        [Test]
        public void WriteMessageTo_StringValue_Matches()
        {
            var expectedValue = "hello";
            var actualValue = expectedValue;
            var constraint = new ObjectTreeEqualConstraint(expectedValue);
            Assert.IsTrue(constraint.Matches(actualValue));

            var writer = new TextMessageWriter();
            constraint.WriteMessageTo(writer);

            var expectedMessage = "  Expected: <LatticeObjectTree.ObjectTree> with root \"hello\"" + Environment.NewLine
                + "  But was:  <LatticeObjectTree.ObjectTree> with root \"hello\"" + Environment.NewLine;
            Assert.AreEqual(expectedMessage, writer.ToString());
        }

        [Test]
        public void WriteMessageTo_StringValue_OneDifference()
        {
            var expectedValue = "hello";
            var actualValue = "world";
            var constraint = new ObjectTreeEqualConstraint(expectedValue);
            Assert.IsFalse(constraint.Matches(actualValue));

            var writer = new TextMessageWriter();
            constraint.WriteMessageTo(writer);

            var expectedMessage = "  Expected: <LatticeObjectTree.ObjectTree> with root \"hello\"" + Environment.NewLine
                + "  But was:  <LatticeObjectTree.ObjectTree> with root \"world\"" + Environment.NewLine
                + "  1 Differences:    < <<root>: expected value \"hello\" but was \"world\".> >";
            Assert.AreEqual(expectedMessage, writer.ToString());
        }
    }
}
