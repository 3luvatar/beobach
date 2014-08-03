using System;
using System.Collections.Generic;
using System.Linq;

namespace Beobach.Observables
{
    internal interface IObservableProperty
    {
        ObservableSubscription Subscribe(Action subscriptionCallback, object subscriber);
        ObservableSubscription Subscribe(Action subscriptionCallback, string notificationType, object subscriber);

        void NotifySubscribers(object newVal);
        bool IsNotifying(string notificationType);
        bool IsAccessed { get; set; }
        object Value { get; }
    }

    public abstract class ObservableProperty
    {
        public const string VALUE_CHANGED_EVENT = "valueChanged";
        public const string BEFORE_VALUE_CHANGED_EVENT = "beforeValueChanged";
        public const string ARRAY_CHANGE = "arrayChange";
    }

    public class ObservableProperty<T> : ObservableProperty, IObservableProperty
    {
        internal readonly Dictionary<string, List<ObservableSubscription>> _subscribers =
            new Dictionary<string, List<ObservableSubscription>>();

        internal readonly HashSet<string> isNotifiying = new HashSet<string>();

        public ObservableProperty()
        {
        }

        public ObservableProperty(T value)
        {
            Value = value;
        }

        protected T _value;

        public bool IsAccessed { get; set; }

        public virtual T Value
        {
            get
            {
                NotificationHelper.ValueAccessed(this);
                return _value;
            }
            set
            {
                NotifySubscribers(_value, BEFORE_VALUE_CHANGED_EVENT);
                _value = value;
                if (!IsAccessed)
                    NotifySubscribers(_value);
            }
        }

        public virtual T Peek()
        {
            return _value;
        }

        object IObservableProperty.Value
        {
            get { return Value; }
        }

        public int SubscriptionsCount
        {
            get { return _subscribers.Values.SelectMany(list => list).Count(subscription => !subscription.Removed); }
        }

        public bool HasSubscribers
        {
            get { return _subscribers.Values.Any(list => list.Any(subscription => !subscription.Removed)); }
        }

        public bool HasSubscribersType(string notificationType)
        {
            List<ObservableSubscription> subscribers;
            if (!_subscribers.TryGetValue(notificationType, out subscribers)) return false;
            return subscribers.Any(subscription => !subscription.Removed);
        }

        public void NotifySubscribers(T newVal)
        {
            NotifySubscribers(newVal, VALUE_CHANGED_EVENT);
        }

        void IObservableProperty.NotifySubscribers(object newVal)
        {
            NotifySubscribers((T) newVal);
        }

        public virtual void NotifySubscribers<T_SUB>(T_SUB newVal, string notificationType)
        {
            try
            {
                if (!isNotifiying.Add(notificationType)) return;
                List<ObservableSubscription> subscribers;
                if (!_subscribers.TryGetValue(notificationType, out subscribers)) return;

                var notified = new List<IObservableProperty>();
                for (int i = 0; i < subscribers.Count; i++)
                {
                    ObservableSubscription subscriber = subscribers[i];

                    if (subscriber.Removed || //don't notify if removed
                        subscriber.IsSubscriberNotifying(notificationType) ||
                        //don't notify if is already notifying to prevent loops
                        notified.Any(subscriber.Subscriber)) continue; //dont notify when already notified
                    var subScriberNotified =
                        NotificationHelper.CatchNotifications(() => subscriber.NotifyChanged(newVal));
                    notified.AddRange(subScriberNotified.OfType<IObservableProperty>());
                }
                subscribers.RemoveAll(subscription => subscription.Removed);
            }
            finally
            {
                isNotifiying.Remove(notificationType);
            }
        }

        bool IObservableProperty.IsNotifying(string notificationType)
        {
            return isNotifiying.Contains(notificationType);
        }

        protected virtual void AddSubscription<T_SUB>(ObservableSubscription<T_SUB> subscription,
            string notificationType)
        {
            List<ObservableSubscription> subscribers;
            if (!_subscribers.TryGetValue(notificationType, out subscribers))
            {
                subscribers = new List<ObservableSubscription>();
                _subscribers.Add(notificationType, subscribers);
            }

            subscribers.Add(subscription);
        }

        ObservableSubscription IObservableProperty.Subscribe(Action subscriptionCallback, object subscriber)
        {
            return ((IObservableProperty) this).Subscribe(subscriptionCallback, VALUE_CHANGED_EVENT, subscriber);
        }

        ObservableSubscription IObservableProperty.Subscribe(Action subscriptionCallback,
            string notificationType,
            object subscriber)
        {
            if (subscriptionCallback == null)
            {
                throw new NullReferenceException("subscriptionCallback may not be null");
            }
            return SubscribeEvent<T>((value) => subscriptionCallback(), notificationType, subscriber);
        }

        public ObservableSubscription<T> Subscribe(SubscriptionCallBack<T> subscriptionCallback, object subscriber)
        {
            return SubscribeEvent(subscriptionCallback, VALUE_CHANGED_EVENT, subscriber);
        }

        public ObservableSubscription<T> Subscribe(SubscriptionCallBack<T> subscriptionCallback,
            string notificationType,
            object subscriber)
        {
            return SubscribeEvent(subscriptionCallback, notificationType, subscriber);
        }

        public ObservableSubscription<T_SUB> SubscribeEvent<T_SUB>(SubscriptionCallBack<T_SUB> subscriptionCallback,
            string notificationType,
            object subscriber)
        {
            if (subscriptionCallback == null)
            {
                throw new NullReferenceException("subscriptionCallback may not be null");
            }
            ObservableSubscription<T_SUB> subscription = new ObservableSubscription<T_SUB>(this,
                subscriptionCallback,
                subscriber);
            AddSubscription(subscription, notificationType);
            return subscription;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name ?? string.Format("Value: {0}, Type:{1}", Peek(), GetType().Name);
        }

        public static implicit operator T(ObservableProperty<T> property)
        {
            return property.Value;
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }
    }
}