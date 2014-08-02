using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beobach
{
    internal interface IObservableProperty
    {
        ObservableSubscription Subscribe(Action subscriptionCallback);
        ObservableSubscription Subscribe(Action subscriptionCallback, string notificationType);

        void NotifySubscribers(object newVal);
        bool IsAccessed { get; set; }
        object Value { get; }
    }

    public abstract class ObservableProperty
    {
        public const string VALUE_CHANGED_EVENT = "valueChanged";
        public const string BEFORE_VALUE_CHANGED_EVENT = "beforeValueChanged";
    }

    public class ObservableProperty<T> : ObservableProperty, IObservableProperty
    {
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
                BeobachHelper.ValueAccessed(this);
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

        public T Peek()
        {
            return _value;
        }


        object IObservableProperty.Value
        {
            get { return Value; }
        }

        protected readonly Dictionary<string, List<ObservableSubscription<T>>> _subscribers =
            new Dictionary<string, List<ObservableSubscription<T>>>();

        public void NotifySubscribers(T newVal)
        {
            NotifySubscribers(newVal, VALUE_CHANGED_EVENT);
        }

        public virtual void NotifySubscribers(T newVal, string notificationType)
        {
            List<ObservableSubscription<T>> subscribers;
            if (!_subscribers.TryGetValue(notificationType, out subscribers)) return;

            foreach (var subscriber in subscribers.ToArray())
            {
                if (!subscriber.Removed)
                {
                    subscriber.NotifyChanged(newVal);
                }
            }
            subscribers.RemoveAll(subscription => subscription.Removed);
        }

        void IObservableProperty.NotifySubscribers(object newVal)
        {
            NotifySubscribers((T) newVal);
        }

        private void AddSubscription(ObservableSubscription<T> subscription, string notificationType)
        {
            List<ObservableSubscription<T>> subscribers;
            if (!_subscribers.TryGetValue(notificationType, out subscribers))
            {
                subscribers = new List<ObservableSubscription<T>>();
                _subscribers.Add(notificationType, subscribers);
            }

            subscribers.Add(subscription);
        }

        ObservableSubscription IObservableProperty.Subscribe(Action subscriptionCallback)
        {
            return ((IObservableProperty) this).Subscribe(subscriptionCallback, VALUE_CHANGED_EVENT);
        }

        ObservableSubscription IObservableProperty.Subscribe(Action subscriptionCallback, string notificationType)
        {
            if (subscriptionCallback == null)
            {
                throw new NullReferenceException("subscriptionCallback may not be null");
            }
            return Subscribe((value) => subscriptionCallback(), notificationType);
        }


        public ObservableSubscription<T> Subscribe(SubscriptionCallBack<T> subscriptionCallback)
        {
            return Subscribe(subscriptionCallback, VALUE_CHANGED_EVENT);
        }

        public ObservableSubscription<T> Subscribe(SubscriptionCallBack<T> subscriptionCallback,
            string notificationType)
        {
            if (subscriptionCallback == null)
            {
                throw new NullReferenceException("subscriptionCallback may not be null");
            }
            ObservableSubscription<T> subscription = new ObservableSubscription<T>(this, subscriptionCallback);
            AddSubscription(subscription, notificationType);
            return subscription;
        }

        internal ObservableSubscription Subscribe(IObservableProperty property)
        {
            if (property == null)
            {
                throw new NullReferenceException("property may not be null");
            }
            return Subscribe((value) => property.NotifySubscribers(property.Value), VALUE_CHANGED_EVENT);
        }

        public override string ToString()
        {
            return string.Format("Value: {0}, Type:{1}", Peek(), GetType().Name);
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