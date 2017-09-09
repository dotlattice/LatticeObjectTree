using NUnit.Framework;
using TestAttribute = Xunit.FactAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatticeObjectTree.Comparison
{
    public class ObjectTreeNodeDifferenceTest
    {
        [Test]
        public void Constructor_NullExpectedNode()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ObjectTreeNodeDifference(
                    expected: null, 
                    actual: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                    message: string.Empty
                )
            );
        }

        [Test]
        public void Constructor_NullActualNode()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ObjectTreeNodeDifference(
                    expected: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                    actual: null,
                    message: string.Empty
                )
            );
        }

        [Test]
        public void Constructor_EmptyNodesAndNullMessage()
        {
            var actual = new ObjectTreeNodeDifference(
                expected: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                actual: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                message: null
            );
            Assert.AreEqual(string.Empty, actual.Message);
            Assert.IsNotNull(actual.ExpectedNode);
            Assert.IsNotNull(actual.ActualNode);
            Assert.IsNull(actual.ExpectedDisplayValue);
            Assert.IsNull(actual.ActualDisplayValue);
        }

        [Test]
        public void Constructor_NullDisplayValues()
        {
            var actual = new ObjectTreeNodeDifference(
                expected: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                expectedDisplayValue: null,
                actual: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                actualDisplayValue: null,
                messageFormat: string.Empty
            );
            Assert.IsNull(actual.ExpectedDisplayValue);
            Assert.IsNull(actual.ActualDisplayValue);
        }

        [Test]
        public void Message_UnformattedMessage()
        {
            var diff = new ObjectTreeNodeDifference(
                expected: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                actual: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                message: "Expected was '{0}', actual was '{1}'"
            );
            var actual = diff.Message;
            Assert.AreEqual("Expected was '{0}', actual was '{1}'", actual);
        }

        [Test]
        public void Message_InvalidForStringFormat()
        {
            var diff = new ObjectTreeNodeDifference(
                expected: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                actual: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                message: "Expected }{ was '{0}', actual was '{1}'"
            );
            var actual = diff.Message;
            Assert.AreEqual("Expected }{ was '{0}', actual was '{1}'", actual);
        }

        [Test]
        public void Message_FormatValues()
        {
            var diff = new ObjectTreeNodeDifference(
                expected: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                expectedDisplayValue: "e",
                actual: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                actualDisplayValue: "a",
                messageFormat: "Expected was '{0}', actual was '{1}'"
            );
            var actual = diff.Message;
            Assert.AreEqual("Expected was 'e', actual was 'a'", actual);
        }

        [Test]
        public void Message_FormatValues_TooManyParameters()
        {
            var diff = new ObjectTreeNodeDifference(
                expected: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                expectedDisplayValue: "e",
                actual: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                actualDisplayValue: "a",
                messageFormat: "Expected was '{0}', actual was '{1}', bonus was '{2}'"
            );
            var actual = diff.Message;
            Assert.AreEqual("Expected was '{0}', actual was '{1}', bonus was '{2}'", actual);
        }

        [Test]
        public void GenerateMessage_ValuesNotUsed()
        {
            var diff = new ObjectTreeNodeDifference(
                expected: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                expectedDisplayValue: "test1",
                actual: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                actualDisplayValue: "test2",
                messageFormat: "This is the message"
            );
            var actual = diff.GenerateMessage("test3", "test4");
            Assert.AreEqual("This is the message", actual);
        }

        [Test]
        public void GenerateMessage_CustomValues()
        {
            var diff = new ObjectTreeNodeDifference(
                expected: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                expectedDisplayValue: "test1",
                actual: new ObjectTreeNode(null, ObjectTreeNodeType.Object),
                actualDisplayValue: "test2",
                messageFormat: "Expected was '{0}', actual was '{1}'"
            );
            var actual = diff.GenerateMessage("test3", "test4");
            Assert.AreEqual("Expected was 'test3', actual was 'test4'", actual);
        }
    }
}
