using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Beobach
{
    public class ComputedObservable<T> : ObservableProperty<T>
    {
        private readonly WriteCallBack<T> _writeCallBack;
        private readonly ComputeCallBack<T> _computeCallBack;
        private bool isValid = false;

        public ComputedObservable(ComputeCallBack<T> computeCallBack) : this(computeCallBack, null, false)
        {
        }

        public ComputedObservable(ComputeCallBack<T> computeCallBack, bool deferEvaluation)
            : this(computeCallBack, null, deferEvaluation)
        {
        }

        public ComputedObservable(ComputeCallBack<T> computeCallBack, WriteCallBack<T> writeCallBack)
            : this(computeCallBack, writeCallBack, false)
        {
        }

        public ComputedObservable(ComputeCallBack<T> computeCallBack, WriteCallBack<T> writeCallBack,
            bool deferEvaluation)
        {
            _computeCallBack = computeCallBack;
            _writeCallBack = writeCallBack;
            if (!deferEvaluation)
            {
                var tmp = Value; //force value to be computed
            }
        }

        public override T Value
        {
            get
            {
                BeobachHelper.ValueAccessed(this);
                if (isValid) return _value;
                ClearComputedSubscriptions();
                NotifySubscribers(_value, BEFORE_VALUE_CHANGED_EVENT);
                BeobachHelper.OnValueAccessed += SubscribeComputed;
                Debug.WriteLine("Observable Computed");
                _value = _computeCallBack();
                BeobachHelper.OnValueAccessed -= SubscribeComputed;
                isValid = true;
                return _value;
            }
            set
            {
                if (IsReadOnly) throw new NotSupportedException("observable is read only");
                _writeCallBack(value);
                NotifySubscribers(Value);
            }
        }


        private void ClearComputedSubscriptions()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        //observables this computed is subscribed to
        private readonly List<ObservableSubscription> _subscriptions = new List<ObservableSubscription>();

        public int DependencyCount
        {
            get { return _subscribers.Count; }
        }

        private void SubscribeComputed(IObservableProperty property)
        {
            if (_subscriptions.Any(subscription => subscription.Equals(property))) return;
            _subscriptions.Add(property.Subscribe(() =>
            {
                isValid = false;
                var val = Value;
                NotifySubscribers(Value);
            }));
        }

        public override bool IsReadOnly
        {
            get { return _writeCallBack == null; }
        }
    }

    public delegate T ComputeCallBack<out T>();

    public delegate void WriteCallBack<in T>(T value);
}