using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beobach;
using Beobach.Observables;
using Beobach.Subscriptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeobachUnitTests
{
    [TestClass]
    public class ArrayTests
    {
        private ObservableList<int> _observableList;
        private int[] initalList = new[] {1, 2, 3, 4, 0};

        [TestInitialize]
        public void InitTest()
        {
            _observableList = new ObservableList<int>(initalList);
        }

        [TestMethod]
        public void TestHoldValues()
        {
            CollectionAssert.AreEquivalent(initalList, _observableList.Value);
            _observableList.Add(5);
            CollectionAssert.Contains(_observableList.Value, 5);
            _observableList.Remove(5);
            CollectionAssert.DoesNotContain(_observableList.Value, 5);
        }

        [TestMethod]
        public void TestSingleChangeNotify()
        {
            IList<ArrayChange<int>> changes = null;
            _observableList.SubscribeArrayChange(value => { changes = value; }, "test");
            int expectedIndex = _observableList.Count;
            _observableList.Add(10);
            CollectionAssert.AreEquivalent(new[] {new ArrayChange<int>(ArrayChangeType.add, 10, expectedIndex)},
                (ICollection) changes);

            CollectionAssert.Contains(_observableList.Value, 10);
            _observableList.Remove(10);
            CollectionAssert.AreEquivalent(new[] {new ArrayChange<int>(ArrayChangeType.remove, 10, expectedIndex)},
                (ICollection) changes);
            CollectionAssert.DoesNotContain(_observableList.Value, 10);
        }

        [TestMethod]
        public void TestInsertChangeNotify()
        {
            var list = new ObservableList<string>("A", "B", "C", "D");
            IList<ArrayChange<string>> changes = null;
            list.SubscribeArrayChange(value => { changes = value; }, "test");
            int expectedIndex = 2;
            list.Insert(expectedIndex, "Added");
            CollectionAssert.Contains(list.Value, "Added");
            CollectionAssert.AreEquivalent(new[]
            {
                new ArrayChange<string>(ArrayChangeType.add, "Added", expectedIndex),
                new ArrayChange<string>(ArrayChangeType.remove, "C", 2),
                new ArrayChange<string>(ArrayChangeType.add, "C", 3),
                new ArrayChange<string>(ArrayChangeType.remove, "D", 3),
                new ArrayChange<string>(ArrayChangeType.add, "D", 4),
            },
                (ICollection) changes);
        }

        [TestMethod]
        public void TestInsertRange()
        {
            var list = new ObservableList<string>("A", "B", "C", "D", "E");
            IList<ArrayChange<string>> changes = null;
            list.SubscribeArrayChange(value => { changes = value; }, "test");
            list.InsertRange(1, Enumerable.Repeat("TEST", 3).Select((s, i) => s + "_" + i));
            CollectionAssert.IsSubsetOf(new[] {"TEST_0", "TEST_1", "TEST_2"}, list.Value);
            CollectionAssert.AreEquivalent(new[]
            {
                new ArrayChange<string>(ArrayChangeType.add, "TEST_0", 1),
                new ArrayChange<string>(ArrayChangeType.add, "TEST_1", 2),
                new ArrayChange<string>(ArrayChangeType.add, "TEST_2", 3),
                new ArrayChange<string>(ArrayChangeType.remove, "B", 1),
                new ArrayChange<string>(ArrayChangeType.add, "B", 4),
                new ArrayChange<string>(ArrayChangeType.remove, "C", 2),
                new ArrayChange<string>(ArrayChangeType.add, "C", 5),
                new ArrayChange<string>(ArrayChangeType.remove, "D", 3),
                new ArrayChange<string>(ArrayChangeType.add, "D", 6),
                new ArrayChange<string>(ArrayChangeType.remove, "E", 4),
                new ArrayChange<string>(ArrayChangeType.add, "E", 7),
            },
                (ICollection) changes);
        }

        [TestMethod]
        public void TestRemoveRange()
        {
            var list = new ObservableList<string>("A", "B", "C", "D", "E");
            IList<ArrayChange<string>> changes = null;
            list.SubscribeArrayChange(value => { changes = value; }, "test");
            CollectionAssert.IsSubsetOf(new[] {"B", "C"}, list.Value);
            list.RemoveRange(1, 2);
            CollectionAssert.IsNotSubsetOf(new[] {"B", "C"}, list.Value);
            CollectionAssert.AreEquivalent(new[]
            {
                new ArrayChange<string>(ArrayChangeType.remove, "B", 1),
                new ArrayChange<string>(ArrayChangeType.remove, "C", 2),
                new ArrayChange<string>(ArrayChangeType.add, "D", 1),
                new ArrayChange<string>(ArrayChangeType.remove, "D", 3),
                new ArrayChange<string>(ArrayChangeType.add, "E", 2),
                new ArrayChange<string>(ArrayChangeType.remove, "E", 4),
            },
                (ICollection) changes);
        }

        [TestMethod]
        public void TestPop()
        {
            var list = new ObservableList<string>("A", "B", "C", "D", "E");
            IList<ArrayChange<string>> changes = null;
            list.SubscribeArrayChange(value => { changes = value; }, "test");
            CollectionAssert.Contains(list.Value, "E");
            Assert.AreEqual(5, list.Count);
            var popedValue = list.Pop();
            Assert.AreEqual("E", popedValue);
            CollectionAssert.DoesNotContain(list.Value, "E");
            Assert.AreEqual(4, list.Count);
            CollectionAssert.AreEquivalent(new[] {new ArrayChange<string>(ArrayChangeType.remove, "E", 4)},
                (ICollection) changes);
        }

        [TestMethod]
        public void TestSort()
        {
            var list = new ObservableList<string>("E", "B", "D", "C", "A");
            IList<ArrayChange<string>> changes = null;
            list.SubscribeArrayChange(value => { changes = value; }, "test");
            list.Sort(1, 3, StringComparer.InvariantCultureIgnoreCase);
            CollectionAssert.AreEqual(new[] {"E", "B", "C", "D", "A"}, list.Value);
            CollectionAssert.AreEquivalent(new[]
            {
                new ArrayChange<string>(ArrayChangeType.add, "C", 2),
                new ArrayChange<string>(ArrayChangeType.remove, "C", 3),
                new ArrayChange<string>(ArrayChangeType.add, "D", 3),
                new ArrayChange<string>(ArrayChangeType.remove, "D", 2),
            },
                (ICollection) changes);
        }

        [TestMethod]
        public void TestReverse()
        {
            var list = new ObservableList<string>("E", "B", "D", "C", "A");
            IList<ArrayChange<string>> changes = null;
            list.SubscribeArrayChange(value => { changes = value; }, "test");
            list.Reverse(1, 3);
            CollectionAssert.AreEqual(new[] {"E", "C", "D", "B", "A"}, list.Value);
            CollectionAssert.AreEquivalent(new[]
            {
                new ArrayChange<string>(ArrayChangeType.remove, "B", 1),
                new ArrayChange<string>(ArrayChangeType.add, "B", 3),
                new ArrayChange<string>(ArrayChangeType.add, "C", 1),
                new ArrayChange<string>(ArrayChangeType.remove, "C", 3),
            },
                (ICollection) changes);
        }

        [TestMethod]
        public void TestShouldModifyOriginal()
        {
            var originalList = new List<int> {1, 2, 3};
            var observableList = new ObservableList<int>(originalList);
            CollectionAssert.Contains(observableList.Value, 2);
            observableList.Remove(2);
            CollectionAssert.DoesNotContain(originalList, 2);
        }

        [TestMethod]
        public void TestReplaceValue()
        {
            var list = new ObservableList<string>("E", "B", "D", "C", "A");
            IList<ArrayChange<string>> changes = null;
            list.SubscribeArrayChange(value => { changes = value; }, "test");
            list[2] = "X";
            Assert.AreEqual("X", list[2]);
            CollectionAssert.AreEquivalent(new[]
            {
                new ArrayChange<string>(ArrayChangeType.remove, "D", 2),
                new ArrayChange<string>(ArrayChangeType.add, "X", 2),
            },
                (ICollection) changes);
        }
    }
}