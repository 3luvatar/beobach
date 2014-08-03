using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        private readonly List<ObservableSubscription> _subscriptions = new List<ObservableSubscription>();
        private HashSet<IObservableProperty> _subscribedObservables = new HashSet<IObservableProperty>();

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

            var accessedProperties = NotificationHelper.CatchValuesAccessed(() => _value = _computeCallBack());
            if (!_isDisposed)
            {
                _subscribedObservables = accessedProperties;
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
            foreach (ObservableSubscription subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscribedObservables.Clear();
        }

        private void UpdateSubscriptions()
        {
            //in DisposeSubscriptions we mark them to be removed, but here we just enable them again
            //if we still need them, then remove any that we dont
            foreach (var observable in _subscribedObservables)
            {
                var subscription =
                    _subscriptions.SingleOrDefault(sub => sub.ForObservable(observable));
                if (subscription != null)
                {
                    subscription.Removed = false;
                }
                else
                {
                    _subscriptions.Add(observable.Subscribe(OnSubscribedPropertyChanged, this));
                }
            }
            _subscriptions.RemoveAll(subscription => subscription.Removed);
        }

        public int DependencyCount
        {
            get { return _subscribedObservables.Count; }
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