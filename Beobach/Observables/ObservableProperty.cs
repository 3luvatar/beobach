using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beobach.Subscriptions;

namespace Beobach.Observables
{
    public interface IObservableProperty
    {
        IObservableSubscription Subscribe(Action<object> subscriptionCallback, object subscriber);

        IObservableSubscription Subscribe(Action<object> subscriptionCallback,
            string notificationType,
            object subscriber);

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
        public const int SUBSCRIBE_ALL_CHANGES_INDEX = -1;
    }

    public class ObservableProperty<T> : ObservableProperty, IObservableProperty
    {
        internal readonly Dictionary<string, List<IObservableSubscription>> _subscribers =
            new Dictionary<string, List<IObservableSubscription>>();

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
                if (!IsPendingNotify)
                {
                    OnNotifySubscribers(_value, BEFORE_VALUE_CHANGED_EVENT);
                    if (HasRateLimiter) OriginalNotifyValue = _value;
                }
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

        protected bool HasSubscribersType(string notificationType)
        {
            List<IObservableSubscription> subscribers;
            if (!_subscribers.TryGetValue(notificationType, out subscribers)) return false;
            return subscribers.Any(subscription => !subscription.Removed);
        }

        public void NotifySubscribers(T newVal)
        {
            OnNotifySubscribers(newVal, VALUE_CHANGED_EVENT);
        }

        void IObservableProperty.NotifySubscribers(object newVal)
        {
            NotifySubscribers((T) newVal);
        }

        private readonly HashSet<string> _isNotifiying = new HashSet<string>();

        public virtual void OnNotifySubscribers<T_SUB>(T_SUB newVal, string notificationType)
        {
            switch (notificationType)
            {
                case VALUE_CHANGED_EVENT:
                    OnNotifyValueChanged((T) ((object) newVal));
                    return;
                case BEFORE_VALUE_CHANGED_EVENT:
                    OnBeforeNotifyValueChanged((T) ((object) newVal));
                    return;
                default:
                    DoNotify(newVal, notificationType);
                    break;
            }
        }

        protected virtual void OnNotifyValueChanged(T newVal)
        {
            if (!HasRateLimiter)
            {
                DoNotify(newVal, VALUE_CHANGED_EVENT);
            }
            else
            {
                NotifyDelay(newVal, VALUE_CHANGED_EVENT);
            }
        }

        protected virtual void OnBeforeNotifyValueChanged(T oldVal)
        {
            DoNotify(oldVal, BEFORE_VALUE_CHANGED_EVENT);
        }

        protected T OriginalNotifyValue
        {
            get { return _originalNotifyValue; }
            set { _originalNotifyValue = value; }
        }

        private void NotifyDelay(T newVal, string notificationType)
        {
            if (IsPendingNotify)
            {
                ValueChangeNotifyCancellationToken.Cancel();
            }
            if (Equals(newVal, OriginalNotifyValue))
            {
                return;
            }
            DelayByRateLimit(() => DoNotify(newVal, notificationType));
        }

        protected void DoNotify<T_SUB>(T_SUB newVal, string notificationType)
        {
            if (!_isNotifiying.Add(notificationType)) return;
            try
            {
                List<IObservableSubscription> subscribers;
                if (!_subscribers.TryGetValue(notificationType, out subscribers)) return;

                var alreadyNotified = new List<IObservableProperty>();
                for (int i = 0; i < subscribers.Count; i++)
                {
                    IObservableSubscription subscriber = subscribers[i];

                    if (!ShouldNotifyChanged(subscriber, notificationType, newVal) ||
                        alreadyNotified.Any(subscriber.Subscriber)) continue; //dont notify when already notified
                    var subscribersNotified =
                        NotificationHelper.CatchNotifications(() => subscriber.NotifyChanged(newVal));
                    alreadyNotified.AddRange(subscribersNotified.OfType<IObservableProperty>());
                }
                subscribers.RemoveAll(subscription => subscription.Removed);
            }
            finally
            {
                _isNotifiying.Remove(notificationType);
            }
        }

        internal virtual bool ShouldNotifyChanged<T_SUB>(IObservableSubscription subscription,
            string notificationType,
            T_SUB newVal)
        {
            //don't notify if removed
            return !subscription.Removed &&
                   !subscription.IsSubscriberNotifying(notificationType);
            //don't notify if is already notifying to prevent loops
        }

        bool IObservableProperty.IsNotifying(string notificationType)
        {
            return _isNotifiying.Contains(notificationType);
        }

        IObservableSubscription IObservableProperty.Subscribe(Action<object> subscriptionCallback, object subscriber)
        {
            return ((IObservableProperty) this).Subscribe(subscriptionCallback, VALUE_CHANGED_EVENT, subscriber);
        }

        IObservableSubscription IObservableProperty.Subscribe(Action<object> subscriptionCallback,
            string notificationType,
            object subscriber)
        {
            if (subscriptionCallback == null)
            {
                throw new NullReferenceException("subscriptionCallback may not be null");
            }
            return SubscribeEvent<T>((value) => subscriptionCallback(value), notificationType, subscriber);
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
            var subscription = new ObservableSubscription<T_SUB>(this, subscriptionCallback, subscriber);
            AddSubscription(subscription, notificationType);
            return subscription;
        }

        protected virtual void AddSubscription<T_SUB>(ObservableSubscription<T_SUB> subscription,
            string notificationType)
        {
            List<IObservableSubscription> subscribers;
            if (!_subscribers.TryGetValue(notificationType, out subscribers))
            {
                subscribers = new List<IObservableSubscription>();
                _subscribers.Add(notificationType, subscribers);
            }
            subscribers.Add(subscription);
        }

        protected int _rateLimit = -1;

        protected virtual bool HasRateLimiter
        {
            get { return _rateLimit > 0; }
        }

        public ObservableProperty<T> RateLimit(int limitMs)
        {
            _rateLimit = limitMs;
            return this;
        }

        protected CancellationTokenSource ValueChangeNotifyCancellationToken;
        private T _originalNotifyValue;

        protected virtual bool IsPendingNotify
        {
            get { return ValueChangeNotifyCancellationToken != null; }
            set { ValueChangeNotifyCancellationToken = value ? new CancellationTokenSource() : null; }
        }

        protected async void DelayByRateLimit(Action action)
        {
            if (IsPendingNotify)
            {
                ValueChangeNotifyCancellationToken.Cancel();
            }
            IsPendingNotify = true;
            try
            {
                await Task.Delay(_rateLimit, ValueChangeNotifyCancellationToken.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            IsPendingNotify = false;
            action();
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name ?? base.ToString();
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