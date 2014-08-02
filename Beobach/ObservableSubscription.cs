using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beobach
{
    internal interface ObservableSubscription : IDisposable
    {
        bool Equals(IObservableProperty property);
    }

    public class ObservableSubscription<T> : ObservableSubscription
    {
        private readonly ObservableProperty<T> _observableProperty;
        private readonly SubscriptionCallBack<T> _subscription;
        internal bool Removed { get; private set; }
        
        public ObservableSubscription(ObservableProperty<T> observableProperty, SubscriptionCallBack<T> subscription)
        {
            _observableProperty = observableProperty;
            _subscription = subscription;
        }

        bool ObservableSubscription.Equals(IObservableProperty property)
        {
            return property == _observableProperty;
        }

        public void Dispose()
        {
            Removed = true;
        }

        public void NotifyChanged(T value)
        {
            if (Removed) return;
            _subscription(value);
        }
    }

    public delegate void SubscriptionCallBack<in T>(T newValue);
}