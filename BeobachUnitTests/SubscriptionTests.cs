using System;
using Beobach;
using Beobach.Observables;
using Beobach.Subscriptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeobachUnitTests
{
    [TestClass]
    public class SubscriptionTests
    {
        [TestMethod]
        public void TestNotifySubscribers()
        {
            var property = new ObservableProperty<string>();
            string notifiedValue = null;
            property.Subscribe(value => notifiedValue = value, "test");
            property.NotifySubscribers("test");
            Assert.AreEqual("test", notifiedValue);
        }

        [TestMethod]
        public void TestUnsubscribe()
        {
            var property = new ObservableProperty<string>("initVal");
            string notifiedValue = null;
            var subscription = property.Subscribe(value => notifiedValue = value, "test");
            subscription.Dispose();
            property.NotifySubscribers("test");
            Assert.AreEqual(null, notifiedValue);
        }

        [TestMethod]
        public void TestPreventNotifyAfterUnsubscribe()
        {
            var property = new ObservableProperty<string>();
            ObservableSubscription<string> subscription1 = null;
            ObservableSubscription<string> subscription2 = null;
            subscription1 = property.Subscribe(value => subscription2.Dispose(), "test");
            bool subscription2WasNotified = false;
            subscription2 = property.Subscribe(value => subscription2WasNotified = true, "test");
            property.NotifySubscribers("ignore");
            Assert.AreEqual(false, subscription2WasNotified);
        }

        [TestMethod]
        public void TestNotifyCustomEvent()
        {
            var property = new ObservableProperty<string>("initVal");
            string notifiedValue = null;
            var subscription = property.SubscribeEvent<string>(value => notifiedValue = value, "myEvent", "test");
            property.NotifySubscribers("bla", "undefinedEvent");
            Assert.IsNull(notifiedValue);
            property.NotifySubscribers("expected", "myEvent");
            Assert.AreEqual("expected", notifiedValue);
        }

        [TestMethod]
        public void TestUnsubscribeCustomEvent()
        {
            var property = new ObservableProperty<string>("initVal");
            string notifiedValue = null;
            var subscription = property.Subscribe(value => notifiedValue = value, "test", "myEvent");
            subscription.Dispose();
            property.NotifySubscribers("ignore", "myEvent");
            Assert.IsNull(notifiedValue);
        }

        [TestMethod]
        public void TestCustomEventIgnoreDefaultNotify()
        {
            var property = new ObservableProperty<string>("initVal");
            string notifiedValue = null;
            var subscription = property.Subscribe(value => notifiedValue = value, "test", "myEvent");
            property.NotifySubscribers("ignore");
            Assert.IsNull(notifiedValue);
        }

        [TestMethod]
        public void TestCircularSubscriptions()
        {
            int timesNotified1 = 0;
            int timesNotified2 = 0;
            var property1 = new ObservableProperty<string>("initVal1");
            var property2 = new ObservableProperty<string>("initVal2");
            property1.Subscribe(value =>
            {
                timesNotified1++;
                property2.Value = value;
            },
                property2);
            property2.Subscribe(value =>
            {
                timesNotified2++;
                property1.Value = value;
            },
                property1);
            Assert.AreEqual(0, timesNotified1);
            Assert.AreEqual(0, timesNotified2);
            Assert.AreEqual("initVal1", property1.Value);
            Assert.AreEqual("initVal2", property2.Value);
            property1.Value = "don't loop!";
            Assert.AreEqual("don't loop!", property1.Value);
            Assert.AreEqual("don't loop!", property2.Value);
            Assert.AreEqual(1, timesNotified1);
            Assert.AreEqual(0, timesNotified2);
        }
    }
}