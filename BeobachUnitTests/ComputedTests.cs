using System;
using System.Collections.Generic;
using Beobach;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeobachUnitTests
{
    [TestClass]
    public class ComputedTests
    {
        [TestMethod]
        public void TestReadValue()
        {
            var observable = new ComputedObservable<string>(() => "expected");
            Assert.AreEqual("expected", observable.Value);
        }

        [TestMethod]
        public void TestImplicitCast()
        {
            var observable = new ComputedObservable<string>(() => "expected");
            string actual = observable;
            Assert.AreEqual("expected", actual);
        }

        [TestMethod]
        public void TestWriteFail()
        {
            var observable = new ComputedObservable<string>(() => "expected");
            bool thrown = false;
            try
            {
                observable.Value = "fail";
            }
            catch (NotSupportedException e)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void TestWriteCallback()
        {
            string wroteValue = null;
            var observable = new ComputedObservable<string>(() => "expected", value => wroteValue = value);
            observable.Value = "expectedVal";
            Assert.AreEqual("expectedVal", wroteValue);
        }

        [TestMethod]
        public void TestCachedEvaluation()
        {
            int timesEvaluated = 0;
            var observable = new ComputedObservable<string>(() =>
            {
                timesEvaluated++;
                return "expected";
            });
            Assert.AreEqual("expected", observable.Value);
            Assert.AreEqual("expected", observable.Value);
            Assert.AreEqual(1, timesEvaluated);
        }

        [TestMethod]
        public void TestUpdateWhenDependencyChanges()
        {
            var property = new ObservableProperty<int>(5);
            var computed = new ComputedObservable<int>(() => property + 1);
            Assert.AreEqual(6, computed.Value);
            property.Value = 10;
            Assert.AreEqual(11, computed.Value);
        }

        [TestMethod]
        public void TestDontDependWithPeek()
        {
            var property = new ObservableProperty<int>(5);
            var computed = new ComputedObservable<int>(() => property.Peek() + 1);
            Assert.AreEqual(6, computed.Value);
            property.Value = 10;
            Assert.AreEqual(6, computed.Value);
        }

        [TestMethod]
        public void TestUnsubscribeOnEachChange()
        {
            var propertyA = new ObservableProperty<string>("A");
            var propertyB = new ObservableProperty<string>("B");
            string propertyToUse = "A";
            int timesEvaluated = 0;
            var computed = new ComputedObservable<string>(() =>
            {
                timesEvaluated++;
                return propertyToUse == "A" ? propertyA.Value : propertyB.Value;
            });
            Assert.AreEqual("A", computed.Value);
            Assert.AreEqual(1, timesEvaluated);

            // Changing an unrelated observable doesn't trigger evaluation
            propertyB.Value = "B2";
            Assert.AreEqual(1, timesEvaluated);

            // Switch to other observable
            propertyToUse = "B";
            propertyA.Value = "A2";
            Assert.AreEqual("B2", computed.Value);
            Assert.AreEqual(2, timesEvaluated);

            // Now changing the first observable doesn't trigger evaluation
            Assert.AreEqual(2, timesEvaluated);
            propertyA.Value = "A3";
        }

        [TestMethod]
        public void TestNotifySubscribers()
        {
            string notifiedValue = null;
            var property = new ObservableProperty<string>("test");
            var computed = new ComputedObservable<string>(() => property + "_computed");
            computed.Subscribe(value => notifiedValue = value);
            Assert.IsNull(notifiedValue);
            property.Value = "is";
            Assert.AreEqual("is_computed", computed.Value);
        }

        [TestMethod]
        public void TestNotifyBeforeChange()
        {
            string notifiedValue = null;
            var property = new ObservableProperty<string>("test");
            var computed = new ComputedObservable<string>(() => property + "_computed");
            computed.Subscribe(value => notifiedValue = value, ObservableProperty.BEFORE_VALUE_CHANGED_EVENT);
            Assert.IsNull(notifiedValue);
            property.Value = "is";
            Assert.AreEqual("test_computed", notifiedValue);
            Assert.AreEqual("is_computed", computed.Value);
        }

        [TestMethod]
        public void TestSingleUpdateOnMultipleCalls()
        {
            var property = new ObservableProperty<int>(2);
            var computed = new ComputedObservable<int>(() => property + property);
            List<int> notifiedValues = new List<int>();
            computed.Subscribe(value => notifiedValues.Add(value));
            Assert.AreEqual(4, computed.Value);
            property.Value = 4;
            Assert.AreEqual(1, notifiedValues.Count);
            Assert.AreEqual(8, notifiedValues[0]);
        }

        [TestMethod]
        public void TestObservableChain()
        {
            var underlyingProperty = new ObservableProperty<int>(1);
            var computed1 = new ComputedObservable<int>(() => 1 + underlyingProperty);
            var computed2 = new ComputedObservable<int>(() => 1 + computed1);
            Assert.AreEqual(3, computed2.Value);
            underlyingProperty.Value = 11;
            Assert.AreEqual(13, computed2.Value);
        }

        [TestMethod]
        public void TestPeekComputed()
        {
            var underlyingProperty = new ObservableProperty<int>(1);
            var computed1 = new ComputedObservable<int>(() => 1 + underlyingProperty);
            var computed2 = new ComputedObservable<int>(() => 1 + computed1.Peek());
            Assert.AreEqual(3, computed2.Value);
            underlyingProperty.Value = 11;
            Assert.AreEqual(3, computed2.Value);
        }

        [TestMethod]
        public void TestDeferEvaluation()
        {
            int timesEvaluated = 0;
            var computed = new ComputedObservable<int>(() =>
            {
                timesEvaluated++;
                return 123;
            }, true);
            Assert.AreEqual(0, timesEvaluated);
            Assert.AreEqual(123, computed.Value);
            Assert.AreEqual(1, timesEvaluated);
        }

        [TestMethod]
        public void TestSubscribeInvokesEvalOnDefer()
        {
            var underlyingProperty = new ObservableProperty<int>(1);
            var computed = new ComputedObservable<int>(() => underlyingProperty.Value, true);
            var result = new ObservableProperty<int>();
            Assert.AreEqual(0, computed.DependencyCount);
            computed.Subscribe(value => result.Value = value);

        }
    }
}