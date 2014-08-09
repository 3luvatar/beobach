using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Beobach.Subscriptions;

namespace Beobach.Observables
{
    public class ComputedObservable<T> : ObservableProperty<T>, IDisposable
    {
        private readonly WriteCallBack<T> _writeCallBack;
        private readonly ComputeCallBack<T> _computeCallBack;
        private bool _isValid;
        private bool _hasBeenAccessed;
        private bool _isDisposed;
        //observables this computed is subscribed to
        private readonly List<IObservableSubscription> _subscriptions = new List<IObservableSubscription>();
        private HashSet<PropertyAccessNotification> _accessNotifications = new HashSet<PropertyAccessNotification>();

        public ComputedObservable(ComputeCallBack<T> computeCallBack, bool deferEvaluation)
            : this(computeCallBack, null, deferEvaluation)
        {
        }

        public ComputedObservable(ComputeCallBack<T> computeCallBack,
            WriteCallBack<T> writeCallBack = null,
            bool deferEvaluation = false)
        {
            _computeCallBack = computeCallBack;
            _writeCallBack = writeCallBack;
            if (!deferEvaluation)
            {
                ComputeValue();
            }
        }

        public override T Value
        {
            get
            {
                if (_isDisposed) return _value;
                NotificationHelper.ValueAccessed(this);
                if (_isValid) return _value;
                ComputeValue();
                return _value;
            }
            set
            {
                if (IsReadOnly) throw new NotSupportedException("observable is read only");
                _writeCallBack(value);
                NotifySubscribers(Value);
            }
        }

        public override T Peek()
        {
            if (!_isValid)
                ComputeValue();
            return base.Peek();
        }

        public void Dispose()
        {
            _isDisposed = true;
            DisposeSubscriptions();
        }

        private void ComputeValue()
        {
            DisposeSubscriptions();
            NotifySubscribers(_value, BEFORE_VALUE_CHANGED_EVENT);
            _hasBeenAccessed = true;

            var accessNotifications = NotificationHelper.CatchValuesAccessed(() => _value = _computeCallBack());
            if (!_isDisposed)
            {
                _accessNotifications = accessNotifications;
                UpdateSubscriptions();
            }
            _isValid = true;
            NotifySubscribers(_value);
        }

        private void OnSubscribedPropertyChanged()
        {
            if (!_isValid) return;
            _isValid = false;
            ComputeValue();
        }

        private void DisposeSubscriptions()
        {
            foreach (IObservableSubscription subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _accessNotifications.Clear();
        }

        private void UpdateSubscriptions()
        {
            //in DisposeSubscriptions we mark them to be removed, but here we just enable them again
            //if we still need them, then remove any that we dont
            foreach (var accessNotification in _accessNotifications)
            {
                PropertyAccessNotification notification = accessNotification;
                var subscriptions = _subscriptions.Where(sub => sub.ForObservable(notification.ObservableProperty));
                bool hasSubscription = false;
                foreach (var subscription in subscriptions)
                {
                    hasSubscription = true;
                    subscription.Removed = false;
                }
                if (!hasSubscription)
                {
                    _subscriptions.Add(accessNotification.CreateSubscription(OnSubscribedPropertyChanged, this));
                }
            }
            _subscriptions.RemoveAll(subscription => subscription.Removed);
        }

        public int DependencyCount
        {
            get { return _accessNotifications.Count; }
        }

        protected override void AddSubscription<T_SUB>(ObservableSubscription<T_SUB> subscription,
            string notificationType)
        {
            if (!_hasBeenAccessed)
            {
                ComputeValue(); //todo note why?
            }
            base.AddSubscription(subscription, notificationType);
        }

        public override bool IsReadOnly
        {
            get { return _writeCallBack == null; }
        }
    }

    public delegate T ComputeCallBack<out T>();

    public delegate void WriteCallBack<in T>(T value);
}