using System.Collections.Generic;
using Beobach.Observables;

namespace Beobach.Subscriptions
{
    internal interface IArrayChangeSubscription : IObservableSubscription
    {
    }

    public class ArrayChangeSubscription<T> : ObservableSubscription<IList<ArrayChange<T>>>, IArrayChangeSubscription
    {
        public ArrayChangeSubscription(ObservableList<T> observableProperty,
            SubscriptionCallBack<IList<ArrayChange<T>>> subscription,
            object subscriber)
            : this(observableProperty, subscription, subscriber, ObservableProperty.SUBSCRIBE_ALL_CHANGES_INDEX)
        {
        }

        public ArrayChangeSubscription(ObservableList<T> observableProperty,
            SubscriptionCallBack<IList<ArrayChange<T>>> subscription,
            object subscriber,
            int subscribedIndex)
            : base(observableProperty, subscription, subscriber)
        {
            SubscribedIndex = subscribedIndex;
        }

        internal int SubscribedIndex { get; private set; }
    }

    public interface ArrayChange
    {
        ArrayChangeType ChangeType { get; }
        int Index { get; }
        object Value { get; }
    }

    public class ArrayChange<T> : ArrayChange
    {
        public ArrayChange(ArrayChangeType changeType, T value, int index)
        {
            ChangeType = changeType;
            Index = index;
            Value = value;
        }

        public ArrayChangeType ChangeType { get; internal set; }
        public int Index { get; internal set; }
        public T Value { get; internal set; }
        object ArrayChange.Value { get { return Value; } }

        protected bool Equals(ArrayChange<T> other)
        {
            return ChangeType == other.ChangeType && Index == other.Index &&
                   EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArrayChange<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) ChangeType;
                hashCode = (hashCode*397) ^ Index;
                hashCode = (hashCode*397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("ChangeType: {0},  Value: {2}, Index: {1}", ChangeType, Index, Value);
        }
    }

    public enum ArrayChangeType
    {
        add,
        remove,
    }
}