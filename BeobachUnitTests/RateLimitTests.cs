using System;
using System.Threading;
using Beobach.Observables;
using Beobach.Subscriptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeobachUnitTests
{
    [TestClass]
    public class RateLimitTests
    {
        [TestMethod]
        public void DelayNotificationTest()
        {
            NotifySpy notifySpy = new NotifySpy();
            ObservableProperty<string> observable = new ObservableProperty<string>().RateLimit(500);
            observable.Subscribe(notifySpy.Call, "test");
            observable.Value = "A";
            Assert.IsFalse(notifySpy.HasBeenCalled);
            Assert.AreEqual("A", observable);
            Thread.Sleep(300);
            observable.Value = "B";
            Assert.AreEqual("B", observable);
            Assert.IsFalse(notifySpy.HasBeenCalled);
            Thread.Sleep(300);
            Assert.IsFalse(notifySpy.HasBeenCalled);
            Thread.Sleep(300);
            Assert.IsTrue(notifySpy.HasBeenCalled);
            Assert.AreEqual("B", notifySpy.CalledValue);
        }

        [TestMethod]
        public void TestChangeRevert()
        {
            NotifySpy notifySpy = new NotifySpy();
            ObservableProperty<string> observable = new ObservableProperty<string>("Z").RateLimit(500);
            observable.Subscribe(notifySpy.Call, "test");
            observable.Value = "A";
            Assert.IsFalse(notifySpy.HasBeenCalled);
            Assert.AreEqual("A", observable);
            Thread.Sleep(300);
            observable.Value = "B";
            Assert.AreEqual("B", observable);
            Assert.IsFalse(notifySpy.HasBeenCalled);
            Thread.Sleep(300);
            observable.Value = "Z";
            Assert.IsFalse(notifySpy.HasBeenCalled);
            Thread.Sleep(600);
            Assert.IsFalse(notifySpy.HasBeenCalled);
        }

        [TestMethod]
        public void TestChangeFromNotify()
        {
            NotifySpy notifySpy = new NotifySpy();
            ObservableProperty<string> observable = new ObservableProperty<string>("Z").RateLimit(500);
            observable.Subscribe(notifySpy.Call, "test");
            ObservableSubscription<string> subscription = null;
            subscription = observable.Subscribe(value =>
            {
                observable.Value = "X";
                subscription.Dispose();
            },
                "test1");
            observable.Value = "A";
            Assert.IsFalse(notifySpy.HasBeenCalled);
            Assert.AreEqual("A", observable);
            Thread.Sleep(550);
            Assert.IsTrue(notifySpy.HasBeenCalled);
            Assert.AreEqual("A", notifySpy.CalledValue);
            Assert.AreEqual("X", observable.Value);
            notifySpy.Reset();
            Thread.Sleep(550);
            Assert.IsTrue(notifySpy.HasBeenCalled);
            Assert.AreEqual("X", notifySpy.CalledValue);
        }

        [TestMethod]
        public void TestNotifyBeforeChangeCalls()
        {
            NotifySpy notifySpy = new NotifySpy();
            ObservableProperty<string> observable = new ObservableProperty<string>("Z").RateLimit(50);
            observable.Subscribe(notifySpy.Call, ObservableProperty.BEFORE_VALUE_CHANGED_EVENT, "test");
            observable.Value = "A";
            Assert.AreEqual("Z", notifySpy.CalledValue);
            notifySpy.Reset();
            observable.Value = "B";
            Assert.IsFalse(notifySpy.HasBeenCalled);
        }

        [TestMethod]
        public void TestComputedSubscriptionNotifyDelay()
        {
            ObservableProperty<string> observable = new ObservableProperty<string>("Z");

            NotifySpy notifySpy = new NotifySpy();
            ComputedObservable<string> computed = new ComputedObservable<string>(() => observable.Value).RateLimit(500);
            computed.Subscribe(notifySpy.Call, "test");
            notifySpy.Reset();
            observable.Value = "A";
            observable.Value = "B";
            Assert.IsFalse(notifySpy.HasBeenCalled);
            Thread.Sleep(510);
            Assert.IsTrue(notifySpy.HasBeenCalled);
            Assert.AreEqual("B", notifySpy.CalledValue);
        }

        [TestMethod]
        public void TestComputedDelayNotifyAndEval()
        {
            ObservableProperty<string> observable = new ObservableProperty<string>("Z");

            NotifySpy notifySpy = new NotifySpy();
            NotifySpy evalSpy = new NotifySpy();
            ComputedObservable<string> computed = new ComputedObservable<string>(() =>
            {
                evalSpy.Call(observable.Value);
                return observable.Value;
            }).RateLimit(500);
            computed.Subscribe(notifySpy.Call, "test");
            evalSpy.Reset();
            observable.Value = "A";
            Assert.IsFalse(evalSpy.HasBeenCalled, "should not have been called as eval is buffered");
            Assert.AreEqual("A", computed.Value, "should force evaluation even though it's buffered");
            Assert.IsTrue(evalSpy.HasBeenCalled, "should have called eval");
            Assert.AreEqual("A", evalSpy.CalledValue, "should match new val");
            Assert.IsFalse(notifySpy.HasBeenCalled, "should not have been notified of change until buffer is over");
            evalSpy.Reset();
            observable.Value = "B";
            Assert.AreEqual("A", computed.Peek(), "peek should use previous value until buffer is over");
            Assert.IsFalse(notifySpy.HasBeenCalled);
            Assert.IsFalse(evalSpy.HasBeenCalled);
            Thread.Sleep(550);
            Assert.IsTrue(notifySpy.HasBeenCalled, "delay should have passed and notify called");
            Assert.AreEqual("B", notifySpy.CalledValue, "should have been notified of value change after timeout");
            Assert.AreEqual("B", evalSpy.CalledValue);
            Assert.AreEqual(1, evalSpy.TimesCalled);
        }

        [TestMethod]
        public void TestComputedChangeRevert()
        {
            NotifySpy notifySpy = new NotifySpy();
            ObservableProperty<string> observable = new ObservableProperty<string>("original");
            ComputedObservable<string> computed = new ComputedObservable<string>(() => observable.Value).RateLimit(500);
            computed.Subscribe(notifySpy.Call, "test");
            observable.Value = "New";
            Assert.AreEqual("New", computed.Value);
            Assert.IsFalse(notifySpy.HasBeenCalled);
            observable.Value = "original";
            Thread.Sleep(550);
            Assert.IsFalse(notifySpy.HasBeenCalled);
            Assert.AreEqual("original", computed.Value);
            Assert.IsFalse(notifySpy.HasBeenCalled);
            observable.Value = "New";
            Thread.Sleep(550);
            Assert.IsTrue(notifySpy.HasBeenCalled);
            Assert.AreEqual("New", notifySpy.CalledValue);
        }
    }

    public class NotifySpy
    {
        public static readonly object DEFALUT_CALLED = new object();

        public NotifySpy()
        {
            CalledValue = DEFALUT_CALLED;
        }

        public void Reset()
        {
            TimesCalled = 0;
            CalledValue = DEFALUT_CALLED;
        }

        public int TimesCalled { get; private set; }
        public object CalledValue { get; private set; }

        public bool HasBeenCalled
        {
            get { return TimesCalled != 0 && CalledValue != DEFALUT_CALLED; }
        }

        public void Call<T>(T value)
        {
            Call((object) value);
        }

        public void Call(object value)
        {
            TimesCalled++;
            CalledValue = value;
        }
    }
}