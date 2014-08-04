using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beobach.Observables;

namespace Beobach
{
    internal class PropertyAccessNotification
    {
        public PropertyAccessNotification(IObservableProperty observableProperty)
        {
            ObservableProperty = observableProperty;
        }

        public IObservableProperty ObservableProperty { get; set; }

        protected bool Equals(PropertyAccessNotification other)
        {
            return Equals(ObservableProperty, other.ObservableProperty);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PropertyAccessNotification) obj);
        }

        public override int GetHashCode()
        {
            return (ObservableProperty != null ? ObservableProperty.GetHashCode() : 0);
        }

        public virtual IObservableSubscription CreateSubscription(Action subscriptionCallback, object subscriber)
        {
            return ObservableProperty.Subscribe(subscriptionCallback, subscriber);
        }

        public override string ToString()
        {
            return ObservableProperty.ToString();
        }
    }

    internal class IndexAccessNotification : PropertyAccessNotification
    {
        public int AccesedIndex { get; set; }

        public IObservableList ObservableProperty
        {
            get { return (IObservableList) base.ObservableProperty; }
            set { base.ObservableProperty = value; }
        }

        public IndexAccessNotification(IObservableList observableProperty, int index) : base(observableProperty)
        {
            AccesedIndex = index;
        }

        public override IObservableSubscription CreateSubscription(Action subscriptionCallback, object subscriber)
        {
            return ObservableProperty.SubscribeIndexChange(subscriptionCallback, subscriber, AccesedIndex);
        }

        protected bool Equals(IndexAccessNotification other)
        {
            return base.Equals(other) && AccesedIndex == other.AccesedIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IndexAccessNotification) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ AccesedIndex;
            }
        }
    }
}