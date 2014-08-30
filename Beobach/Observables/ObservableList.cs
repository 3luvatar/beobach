using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Beobach.Extensions;
using Beobach.Subscriptions;

namespace Beobach.Observables
{
    internal interface IObservableList : IObservableProperty
    {
        IArrayChangeSubscription SubscribeArrayChange(Action subscriptionCallback, object subscriber);
        IArrayChangeSubscription SubscribeIndexChange(Action<object> subscriptionCallback, object subscriber, int index);
    }

    public static class ObservableList
    {
        public static ObservableList<ObservableProperty<T>> New<T>(params T[] values)
        {
            return new ObservableList<ObservableProperty<T>>(values.Select(t => new ObservableProperty<T>(t)));
        }
    }

    public class ObservableList<T> : ObservableProperty<List<T>>, IList<T>, IObservableList
    {
        public ObservableList() : this(new List<T>())
        {
        }

        public ObservableList(List<T> value) : base(value)
        {
        }

        public ObservableList(IEnumerable<T> value)
            : base(value.ToList())
        {
        }

        public ObservableList(params T[] values) : this(values.ToList())
        {
        }

        private bool HasArrayChangeSubscribers
        {
            get { return HasSubscribersType(ARRAY_CHANGE); }
        }

        IArrayChangeSubscription IObservableList.SubscribeArrayChange(Action subscriptionCallback, object subscriber)
        {
            return SubscribeArrayChange(value => subscriptionCallback(), subscriber);
        }

        IArrayChangeSubscription IObservableList.SubscribeIndexChange(Action<object> subscriptionCallback,
            object subscriber,
            int index)
        {
            return SubscribeArrayChange(value => subscriptionCallback(value), subscriber, index);
        }

        public ArrayChangeSubscription<T> SubscribeArrayChange(
            SubscriptionCallBack<IList<ArrayChange<T>>> subscriptionCallback,
            object subscriber)
        {
            return SubscribeArrayChange(subscriptionCallback, subscriber, SUBSCRIBE_ALL_CHANGES_INDEX);
        }

        private ArrayChangeSubscription<T> SubscribeArrayChange(
            SubscriptionCallBack<IList<ArrayChange<T>>> subscriptionCallback,
            object subscriber,
            int index)
        {
            var subscription = new ArrayChangeSubscription<T>(this, subscriptionCallback, subscriber, index);
            AddSubscription(subscription, ARRAY_CHANGE);
            return subscription;
        }

        private static ArrayChange<T> ArrayChange(ArrayChangeType changeType, T value, int index)
        {
            return new ArrayChange<T>(changeType, value, index);
        }

        private void NotifyArrayChange(IList<T> oldList, IList<T> newList)
        {
            if (!HasArrayChangeSubscribers) return;
            List<ArrayChange<T>> changes = new List<ArrayChange<T>>();
            int count = Math.Max(oldList.Count, newList.Count);
            for (int i = 0; i < count; i++)
            {
                if (oldList.Count < i)
                {
                    changes.Add(ArrayChange(ArrayChangeType.add, newList[i], i));
                }
                else if (newList.Count < i)
                {
                    changes.Add(ArrayChange(ArrayChangeType.remove, oldList[i], i));
                }
                else
                {
                    T oldVal = oldList[i];
                    T newVal = newList[i];
                    if (Equals(oldVal, newVal)) continue;
                    changes.Add(ArrayChange(ArrayChangeType.add, newVal, i));
                    changes.Add(ArrayChange(ArrayChangeType.remove, oldVal, i));
                }
            }
            NotifyArrayChange(changes);
        }

        private void NotifyArrayChange(params ArrayChange<T>[] changes)
        {
            NotifyArrayChange(changes.AsEnumerable());
        }

        private void NotifyArrayChange(IEnumerable<ArrayChange<T>> changes)
        {
            if (!HasArrayChangeSubscribers) return;
            OnNotifySubscribers((changes as IList<ArrayChange<T>>) ?? changes.ToList(), ARRAY_CHANGE);
        }

        public override void OnNotifySubscribers<T_SUB>(T_SUB newVal, string notificationType)
        {
            switch (notificationType)
            {
                    case ARRAY_CHANGE:
                    OnNotifyArrayChange((IList<ArrayChange<T>>) newVal);
                    return;
            }
            base.OnNotifySubscribers(newVal, notificationType);
        }

        protected virtual void OnNotifyArrayChange(IList<ArrayChange<T>> changes)
        {
            DoNotify(changes, ARRAY_CHANGE);
        }

        internal override bool ShouldNotifyChanged<T_SUB>(IObservableSubscription subscription,
            string notificationType,
            T_SUB newVal)
        {
            if (notificationType == ARRAY_CHANGE &&
                newVal is IList<ArrayChange<T>> &&
                subscription is ArrayChangeSubscription<T>)
            {
                var arrayChangeSubscription = (ArrayChangeSubscription<T>) subscription;
                var changes = (IList<ArrayChange<T>>) newVal;
                if (arrayChangeSubscription.SubscribedIndex != SUBSCRIBE_ALL_CHANGES_INDEX &&
                    changes.All(change => change.Index != arrayChangeSubscription.SubscribedIndex)) return false;
            }
            return base.ShouldNotifyChanged(subscription, notificationType, newVal);
        }

        public override List<T> Value
        {
            get
            {
                NotificationHelper.IndexAccessed(this, SUBSCRIBE_ALL_CHANGES_INDEX);
                return _value;
            }
            set { base.Value = value; }
        }

        public T this[int index]
        {
            get
            {
                NotificationHelper.IndexAccessed(this, index);
                return _value[index];
            }
            set
            {
                T oldVal = _value[index];
                _value[index] = value;
                NotifySubscribers(_value);
                NotifyArrayChange(ArrayChange(ArrayChangeType.add, value, index),
                    ArrayChange(ArrayChangeType.remove, oldVal, index));
            }
        }

        public T Pop()
        {
            var value = this.Last();
            RemoveAt(_value.Count - 1);
            return value;
        }

        public void Clear()
        {
            var changes =
                _value.Select(
                    (arg, i) => ArrayChange(ArrayChangeType.remove, arg, i))
                    .ToArray();
            _value.Clear();
            NotifyArrayChange(changes);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(_value.Count, collection);
        }

        public void Add(T item)
        {
            Insert(_value.Count, item);
        }

        public void InsertRange(int index, IEnumerable<T> enumerable)
        {
            ICollection<T> collection = enumerable as ICollection<T> ?? enumerable.ToArray();
            int moveOffset = collection.Count;
            int indexOffset = index;
            _value.InsertRange(index, collection);
            var addedChanges =
                collection.Select(
                    (arg1, i) =>
                        ArrayChange(ArrayChangeType.add, arg1, i + indexOffset));
            var movedValues = MovedChanges(index, moveOffset);
            NotifyArrayChange(movedValues.Concat(addedChanges));
        }

        public void Insert(int index, T item)
        {
            _value.Insert(index, item);

            NotifyArrayChange(MovedChanges(index, 1).Concat(new[]
            {
                ArrayChange(ArrayChangeType.add, item, index)
            }));
        }

        /// <summary>
        /// call this after changing array
        /// </summary>
        /// <param name="startIndex">index where items were inserted</param>
        /// <param name="moveOffset">number of values added</param>
        /// <returns></returns>
        private IEnumerable<ArrayChange<T>> MovedChanges(int startIndex, int moveOffset)
        {
            return _value.Skip(startIndex + moveOffset)
                .SelectMany((arg, i) =>
                {
                    int newIndex = i + startIndex + moveOffset;
                    int oldIndex = newIndex - moveOffset;
                    return new[]
                    {
                        ArrayChange(ArrayChangeType.remove, arg, oldIndex),
                        ArrayChange(ArrayChangeType.add, arg, newIndex),
                    };
                });
        }

        public void RemoveRange(int index, int count)
        {
            var removedValues = _value.Skip(index)
                .Take(count)
                .Select((arg, i) => ArrayChange(ArrayChangeType.remove, arg, i + index))
                .ToArray();
            _value.RemoveRange(index, count);

            NotifyArrayChange(MovedChanges(index + count, -count).Concat(removedValues));
        }

        public void RemoveAt(int index)
        {
            var removedChange = ArrayChange(ArrayChangeType.remove, _value[index], index);
            _value.RemoveAt(index);

            NotifyArrayChange(MovedChanges(index + 1, -1).Concat(new[] {removedChange}));
        }

        public int RemoveAll(Predicate<T> match)
        {
            throw new NotImplementedException("change notifications are not yet supported");
            return _value.RemoveAll(match);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public void Sort(Comparison<T> comparison)
        {
            Sort(new FunctorComparer<T>(comparison));
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            var oldList = _value.ToArray();
            _value.Sort(index, count, comparer);
            NotifyArrayChange(oldList, _value);
        }

        public void Sort(IComparer<T> comparer = (IComparer<T>) null)
        {
            Sort(0, _value.Count, comparer);
        }

        public void Reverse(int index, int count)
        {
            _value.Reverse(index, count);
            int arraySize = _value.Count;
            NotifyArrayChange(_value.Skip(index).Take(count).SelectMany((value, i) =>
            {
                int newPos = index + i;
                int oldPos = -newPos + arraySize - 1;
                if (newPos == oldPos) return Enumerable.Empty<ArrayChange<T>>();
                return new[]
                {
                    ArrayChange(ArrayChangeType.remove, value, oldPos),
                    ArrayChange(ArrayChangeType.add, value, newPos)
                };
            }));
        }

        public void Reverse()
        {
            Reverse(0, _value.Count);
        }

        public int Count
        {
            get { return Value.Count; }
        }

        public T[] ToArray()
        {
            return Value.ToArray();
        }

        public int LastIndexOf(T item, int index, int count)
        {
            return Value.LastIndexOf(item, index, count);
        }

        public int LastIndexOf(T item, int index)
        {
            return Value.LastIndexOf(item, index);
        }

        public int LastIndexOf(T item)
        {
            return Value.LastIndexOf(item);
        }

        public int IndexOf(T item, int index, int count)
        {
            return Value.IndexOf(item, index, count);
        }

        public int IndexOf(T item, int index)
        {
            return Value.IndexOf(item, index);
        }

        public int IndexOf(T item)
        {
            return Value.IndexOf(item);
        }

        public List<T> GetRange(int index, int count)
        {
            return Value.GetRange(index, count);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return Value.FindLastIndex(startIndex, count, match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return Value.FindLastIndex(startIndex, match);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return Value.FindLastIndex(match);
        }

        public T FindLast(Predicate<T> match)
        {
            return Value.FindLast(match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return Value.FindIndex(startIndex, count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return Value.FindIndex(startIndex, match);
        }

        public int FindIndex(Predicate<T> match)
        {
            return Value.FindIndex(match);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            Value.CopyTo(index, array, arrayIndex, count);
        }

        public void CopyTo(T[] array)
        {
            Value.CopyTo(array);
        }

        public bool Contains(T item)
        {
            return Value.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class FunctorComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> comparison;

        public FunctorComparer(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return comparison(x, y);
        }
    }
}