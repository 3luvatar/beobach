using System;
using Beobach.Observables;

namespace Beobach.Subscriptions
{
    public interface IObservableSubscription : IDisposable
    {
        bool ForObservable(IObservableProperty property);
        bool Subscriber(IObservableProperty property);
        void NotifyChanged(object value);
        bool Removed { get; set; }
        bool IsSubscriberNotifying(string notificationType);
    }

    public class ObservableSubscription<T> : IObservableSubscription
    {
        private readonly ObservableProperty _observableProperty;
        protected readonly SubscriptionCallBack<T> Subscription;
        private readonly object _subscriber;

        private IObservableProperty observableSubscriber
        {
            get { return _subscriber as IObservableProperty; }
        }

        bool IObservableSubscription.Removed
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

        bool IObservableSubscription.ForObservable(IObservableProperty property)
        {
            return property == _observableProperty;
        }

        bool IObservableSubscription.Subscriber(IObservableProperty property)
        {
            return property == _subscriber;
        }

        bool IObservableSubscription.IsSubscriberNotifying(string notificationType)
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

        void IObservableSubscription.NotifyChanged(object value)
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