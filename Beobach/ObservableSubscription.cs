using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beobach.Observables;

namespace Beobach
{
    internal interface ObservableSubscription : IDisposable
    {
        bool ForObservable(IObservableProperty property);
        bool Subscriber(IObservableProperty property);
        void NotifyChanged(object value);
        bool Removed { get; set; }
        bool IsSubscriberNotifying(string notificationType);
    }

    public class ObservableSubscription<T> : ObservableSubscription
    {
        private readonly ObservableProperty _observableProperty;
        protected SubscriptionCallBack<T> Subscription;
        private readonly object _subscriber;

        private IObservableProperty observableSubscriber
        {
            get { return _subscriber as IObservableProperty; }
        }

        bool ObservableSubscription.Removed
        {
            get { return Removed; }
            set { Removed = value; }
        }

        public bool Removed { get; private set; }

        protected ObservableSubscription(ObservableProperty observableProperty, object subscriber)
        {
            _observableProperty = observableProperty;
            _subscriber = subscriber;
        }

        public ObservableSubscription(ObservableProperty observableProperty,
            SubscriptionCallBack<T> subscription,
            object subscriber)
            : this(observableProperty, subscriber)
        {
            Subscription = subscription;
        }

        bool ObservableSubscription.ForObservable(IObservableProperty property)
        {
            return property == _observableProperty;
        }

        bool ObservableSubscription.Subscriber(IObservableProperty property)
        {
            return property == _subscriber;
        }

        bool ObservableSubscription.IsSubscriberNotifying(string notificationType)
        {
            return observableSubscriber != null && observableSubscriber.IsNotifying(notificationType);
        }

        public void Dispose()
        {
            Removed = true;
        }

        public void NotifyChanged(T value)
        {
            if (Removed) return;
            NotificationHelper.NotificationSent(_subscriber);
            Subscription(value);
        }

        void ObservableSubscription.NotifyChanged(object value)
        {
            NotifyChanged((T) value);
        }

        public override string ToString()
        {
            return string.Format("Subscriber: {0}, ObservableProperty: {1}", _subscriber, _observableProperty);
        }
    }

    public delegate void SubscriptionCallBack<in T>(T newValue);
}