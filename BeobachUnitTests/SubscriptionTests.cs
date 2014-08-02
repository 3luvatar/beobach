using System;
using Beobach;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeobachUnitTests
{
    [TestClass]
    public class SubscriptionTests
    {
        [TestMethod]
        public void TestNotifySubscribers()
        {
            ObservableProperty<string> property = new ObservableProperty<string>();
            string notifiedValue = null;
            property.Subscribe(value => notifiedValue = value);
            property.NotifySubscribers("test");
            Assert.AreEqual("test", notifiedValue);
        }

        [TestMethod]
        public void TestUnsubscribe()
        {
            ObservableProperty<string> property = new ObservableProperty<string>("initVal");
            string notifiedValue = null;
            var subscription = property.Subscribe(value => notifiedValue = value);
            subscription.Dispose();
            property.NotifySubscribers("test");
            Assert.AreEqual(null, notifiedValue);
        }

        [TestMethod]
        public void TestPreventNotifyAfterUnsubscribe()
        {
            ObservableProperty<string> property = new ObservableProperty<string>();
            ObservableSubscription<string> subscription1 = null;
            ObservableSubscription<string> subscription2 = null;
            subscription1 = property.Subscribe(value => subscription2.Dispose());
            bool subscription2WasNotified = false;
            subscription2 = property.Subscribe(value => subscription2WasNotified = true);
            property.NotifySubscribers("ignore");
            Assert.AreEqual(false, subscription2WasNotified);
        }

        [TestMethod]
        public void TestNotifyCustomEvent()
        {
            ObservableProperty<string> property = new ObservableProperty<string>("initVal");
            string notifiedValue = null;
            var subscription = property.Subscribe(value => notifiedValue = value, "myEvent");
            property.NotifySubscribers("bla", "undefinedEvent");
            Assert.IsNull(notifiedValue);
            property.NotifySubscribers("expected", "myEvent");
            Assert.AreEqual("expected", notifiedValue);
        }

        [TestMethod]
        public void TestUnsubscribeCustomEvent()
        {
            ObservableProperty<string> property = new ObservableProperty<string>("initVal");
            string notifiedValue = null;
            var subscription = property.Subscribe(value => notifiedValue = value, "myEvent");
            subscription.Dispose();
            property.NotifySubscribers("ignore", "myEvent");
            Assert.IsNull(notifiedValue);
        }

        [TestMethod]
        public void TestCustomEventIgnoreDefaultNotify()
        {
            ObservableProperty<string> property = new ObservableProperty<string>("initVal");
            string notifiedValue = null;
            var subscription = property.Subscribe(value => notifiedValue = value, "myEvent");
            property.NotifySubscribers("ignore");
            Assert.IsNull(notifiedValue);
        }
    }
}