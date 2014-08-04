using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beobach.Observables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeobachUnitTests
{
    [TestClass]
    public class ComputeArraySubscriptionTests
    {
        [TestMethod]
        public void TestAccessSingleValue()
        {
            var property = new ObservableList<int>(5, 2, 7);
            var computed = new ComputedObservable<int>(() => property[2] + 1);
            Assert.AreEqual(8, computed.Value);
            property[2] = 10;
            Assert.AreEqual(11, computed.Value);
        }

        [TestMethod]
        public void TestComputeSubscriptionOnListOfObservables()
        {
            int timesComputed = 0;
            var list = ObservableList.New(5, 6, 7);
            list.Name = "List";
            var computed = new ComputedObservable<int>(() =>
            {
                timesComputed++;
                return list[2].Value + 1;
            }) {Name = "ComputedVal"};
            Assert.AreEqual(8, computed.Value);
            Assert.AreEqual(1, timesComputed);
            list[0].Value = 1;
            Assert.AreEqual(1, timesComputed);
            list[2].Value = 3;
            Assert.AreEqual(4, computed.Value);
            Assert.AreEqual(2, timesComputed);
            list[1] = new ObservableProperty<int>(10);
            Assert.AreEqual(2, timesComputed);
            list[2] = new ObservableProperty<int>(10);
            Assert.AreEqual(3, timesComputed);
            Assert.AreEqual(11, computed.Value);
        }

        [TestMethod]
        public void TestEnumAddsDependancy()
        {
            var list = new ObservableList<int>(1, 2, 3, 4, 5, 6);
            var computed = new ComputedObservable<double[]>(() => list.Select(i => Math.Pow(i, 2)).ToArray())
            {
                Name = "ComputedVal"
            };
            CollectionAssert.AreEquivalent(new[] {1.0, 4.0, 9.0, 16.0, 25.0, 36.0}, computed.Value);
            Assert.AreEqual(1, computed.DependencyCount);

            list[1] = 10;
            CollectionAssert.AreEquivalent(new[] {1.0, 100.0, 9.0, 16.0, 25.0, 36.0}, computed.Value);
        }

        [TestMethod]
        public void TestIterateDependancyCount()
        {
            int timesComputed = 0;
            var list = new ObservableList<int>(1, 2, 3, 4, 5, 6);
            var computed = new ComputedObservable<double>(() =>
            {
                timesComputed++;
                double agrigateVal = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    agrigateVal += list[i];
                }
                return agrigateVal;
            });
            Assert.AreEqual(21, computed.Value);
            Assert.AreEqual(1, timesComputed);
            //should depend on the array and all the indexes in it
            Assert.AreEqual(7, computed.DependencyCount);
        }
    }
}