using System;
using System.Collections.Generic;
using Beobach;
using Beobach.Observables;
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
                Assert.Fail();
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
            var computed = new ComputedObservable<int>(() => property.Value + 1);
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
            computed.Subscribe(value => notifiedValue = value, "test");
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
            computed.Subscribe(value => notifiedValue = value, ObservableProperty.BEFORE_VALUE_CHANGED_EVENT, "test");
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
            Assert.AreEqual(1, computed.DependencyCount);
            List<int> notifiedValues = new List<int>();
            computed.Subscribe(value => notifiedValues.Add(value), "test");
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
        public void TestSingleUpdateOnChain()
        {
            int timesEvaluated = 0;
            var underlyingPropertyLeft = new ObservableProperty<int>(1) {Name = "Left"};
            var underlyingPropertyRight = new ObservableProperty<int>(1) {Name = "Right"};
            var computed1 = new ComputedObservable<int>(() => underlyingPropertyRight + underlyingPropertyLeft)
            {
                Name = "Compute1"
            };
            var computed2 = new ComputedObservable<int>(() =>
            {
                timesEvaluated++;
                return underlyingPropertyLeft + computed1 + underlyingPropertyRight;
            })
            {
                Name = "Compute2"
            };
            Assert.AreEqual(1, timesEvaluated);
            Assert.AreEqual(4, computed2);
            underlyingPropertyLeft.Value = 2;
            Assert.AreEqual(6, computed2);
            Assert.AreEqual(2, timesEvaluated);
            underlyingPropertyRight.Value = 2;
            Assert.AreEqual(8, computed2);
            Assert.AreEqual(3, timesEvaluated);
        }

        [TestMethod]
        public void TestSubscriptionCounts()
        {
            var underlyingPropertyLeft = new ObservableProperty<int>(1) {Name = "Left"};
            var underlyingPropertyRight = new ObservableProperty<int>(1) {Name = "Right"};
            var simpleComputed = new ComputedObservable<int>(() => underlyingPropertyLeft + 5, true);
            var layerdComputed = new ComputedObservable<int>(() => simpleComputed + underlyingPropertyRight);
            Assert.AreEqual(simpleComputed.DependencyCount, 1);
            Assert.AreEqual(layerdComputed.DependencyCount, 2);
        }

        [TestMethod]
        public void TestPeekComputed()
        {
            var underlyingProperty = new ObservableProperty<int>(1);
            var computed1 = new ComputedObservable<int>(() => 1 + underlyingProperty);
            var computed2 = new ComputedObservable<int>(() => 1 + computed1.Peek());
            Assert.AreEqual(3, computed2.Value);
            Assert.AreEqual(0, computed2.DependencyCount);
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
            },
                true);
            Assert.AreEqual(0, timesEvaluated);
            Assert.AreEqual(123, computed.Value);
            Assert.AreEqual(1, timesEvaluated);
        }

        [TestMethod]
        public void TestPeekWithDeferEvaluation()
        {
            int timesEvaluated = 0;
            var computed = new ComputedObservable<int>(() =>
            {
                timesEvaluated++;
                return 123;
            },
                true);
            Assert.AreEqual(0, timesEvaluated);
            Assert.AreEqual(123, computed.Peek());
            Assert.AreEqual(123, computed.Peek());
            Assert.AreEqual(1, timesEvaluated);
        }

        [TestMethod]
        public void TestSubscribeInvokesEvalOnDefer()
        {
            var underlyingProperty = new ObservableProperty<int>(1);
            var computed = new ComputedObservable<int>(() => underlyingProperty.Value, true);
            var result = new ObservableProperty<int>();
            Assert.AreEqual(0, computed.DependencyCount);
            computed.Subscribe(value => result.Value = value, "test");
            Assert.AreEqual(1, computed.DependencyCount);
            Assert.AreEqual(0, result.Value);
            underlyingProperty.Value = 42;
            Assert.AreEqual(42, result.Value);
        }

        [TestMethod]
        public void TestPreventSubscribeViewChangeNotification()
        {
            var underlyingProperty = new ObservableProperty<int>(1);
            var independentProperty = new ObservableProperty<int>(1);
            var computed = new ComputedObservable<int>(() => underlyingProperty.Value);
            Assert.AreEqual(1, computed.DependencyCount);
            computed.Subscribe(value => { var tmp = independentProperty.Value; }, "test");
            underlyingProperty.Value = 2;
            Assert.AreEqual(1, computed.DependencyCount);
            computed.Subscribe(value => { var tmp = independentProperty.Value; },
                ObservableProperty.BEFORE_VALUE_CHANGED_EVENT,
                "test");
            underlyingProperty.Value = 3;
            Assert.AreEqual(1, computed.DependencyCount);
        }

        [TestMethod]
        public void TestLongChains()
        {
            try
            {
                int depth = 1000;
                var first = new ObservableProperty<int>(0);
                var last = first;
                for (int i = 0; i < depth; i++)
                {
                    var previous = last;
                    last = new ComputedObservable<int>(() => previous.Value + 1);
                }
                var all = new ComputedObservable<int>(() => first.Value + last.Value);
                first.Value = 1;
                Assert.AreEqual(depth + 2, all.Value);
            }
            catch (StackOverflowException)
            {
                Assert.Fail("Stack overflow");
            }
        }

        [TestMethod]
        public void TestEvaluateAfterDisposed()
        {
            int timesEvaluated = 0;
            var underlyingProperty = new ObservableProperty<int>(1);
            var computed = new ComputedObservable<int>(() =>
            {
                timesEvaluated++;
                return underlyingProperty.Value;
            });
            Assert.AreEqual(1, timesEvaluated);
            computed.Dispose();
            underlyingProperty.Value = 2;
            Assert.AreEqual(1, timesEvaluated);
            Assert.AreEqual(1, computed.Value);
            Assert.AreEqual(0, computed.DependencyCount);
        }

        [TestMethod]
        public void TestNotAddingDependencyWhenDisposedDuringEvaluation()
        {
            int timesEvaluated = 0;
            var underlyingProperty = new ObservableProperty<int>(1);
            var propertyToTrigerDipose = new ObservableProperty<bool>(false);
            ComputedObservable<int> computed = null;
            computed = new ComputedObservable<int>(() =>
            {
                if (propertyToTrigerDipose.Value)
                {
                    computed.Dispose();
                }
                timesEvaluated++;
                return underlyingProperty.Value;
            });
            Assert.AreEqual(1, timesEvaluated);
            Assert.AreEqual(1, computed.Value);
            Assert.AreEqual(2, computed.DependencyCount);
            Assert.AreEqual(1, underlyingProperty.SubscriptionsCount);
            propertyToTrigerDipose.Value = true;
            Assert.AreEqual(2, timesEvaluated);
            Assert.AreEqual(1, computed.Value);
            Assert.AreEqual(0, computed.DependencyCount);
            Assert.AreEqual(0, underlyingProperty.SubscriptionsCount);
        }

        [TestMethod]
        public void TestPreventRecursionOnSetDependency()
        {
            int timesEvaluated = 0;
            var underlyingProperty = new ObservableProperty<int>(1);
            var computed = new ComputedObservable<int>(() =>
            {
                timesEvaluated++;
                underlyingProperty.Value = (underlyingProperty.Value + 1);
                return 1;
            });
            Assert.AreEqual(1, timesEvaluated);
        }

        

        [TestMethod]
        public void TestTwoWayComputed()
        {
            var testEnumProperty = new ObservableProperty<TestEnum>();
            var computed = Observe.TwoWayComputed(() => testEnumProperty);
            TestEnum lastNotifyValue = TestEnum.two;
            computed.Subscribe(value => lastNotifyValue = value, "test");
            Assert.AreEqual(TestEnum.one, computed.Value);
            testEnumProperty.Value = TestEnum.two;
            Assert.AreEqual(TestEnum.two, computed.Value);
            Assert.AreEqual(TestEnum.two, lastNotifyValue);
            testEnumProperty.Value = TestEnum.one;
            Assert.AreEqual(TestEnum.one, computed.Value);
            Assert.AreEqual(TestEnum.one, lastNotifyValue);
        }

        [TestMethod]
        public void TestComplexTwoWayBinding()
        {
            var kevin = new TestModel("Kevin") ;
            var jacob = new TestModel("Jacob") {TestEnumValue = { Value = TestEnum.three}};
            var observableObject = new ObservableProperty<TestModel>(kevin);

            var testEnumObserver = Observe.TwoWayComputed(() => observableObject.Value.TestEnumValue);
            Assert.AreEqual(TestEnum.one, testEnumObserver.Value);
            testEnumObserver.Value = TestEnum.two;
            Assert.AreEqual(TestEnum.two, kevin.TestEnumValue.Value);
            observableObject.Value = jacob;
            Assert.AreEqual(TestEnum.three, testEnumObserver.Value);
            testEnumObserver.Value = TestEnum.one;
            
            observableObject.Value = kevin;
            Assert.AreEqual(TestEnum.two, testEnumObserver.Value);
        }
    }
}