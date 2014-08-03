using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beobach.Extensions;

namespace Beobach.Observables
{
    public class ObservableList<T> : ObservableProperty<List<T>>, IList<T>
    {
        public ObservableList() : this(new List<T>())
        {
        }

        public ObservableList(List<T> value) : base(value)
        {
        }

        public ObservableList(params T[] values) : this(values.ToList())
        {
        }

        private bool HasArrayChangeSubscribers
        {
            get { return HasSubscribersType(ARRAY_CHANGE); }
        }

        public ArrayChangeSubscription<T> SubscribeArrayChange(
            SubscriptionCallBack<IList<ArrayChange<T>>> subscriptionCallback,
            object subscriber)
        {
            ArrayChangeSubscription<T> subscription = new ArrayChangeSubscription<T>(this,
                subscriptionCallback,
                subscriber);
            AddSubscription(subscription, ARRAY_CHANGE);
            return subscription;
        }

        private static ArrayChange<T> ArrayChange(ArrayChangeType changeType, T value, int index)
        {
            return new ArrayChange<T> {ChangeType = changeType, Value = value, Index = index};
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
            NotifySubscribers((changes as IList<ArrayChange<T>>) ?? changes.ToList(), ARRAY_CHANGE);
        }

        public override List<T> Value
        {
            get { return base.Value; } //TODO check on mutating the list externally
            set { base.Value = value; }
        }

        public T this[int index]
        {
            get { return _value[index]; }
            set { _value[index] = value; }
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
            get { return _value.Count; }
        }

        public T[] ToArray()
        {
            return _value.ToArray();
        }

        public int LastIndexOf(T item, int index, int count)
        {
            return _value.LastIndexOf(item, index, count);
        }

        public int LastIndexOf(T item, int index)
        {
            return _value.LastIndexOf(item, index);
        }

        public int LastIndexOf(T item)
        {
            return _value.LastIndexOf(item);
        }

        public int IndexOf(T item, int index, int count)
        {
            return _value.IndexOf(item, index, count);
        }

        public int IndexOf(T item, int index)
        {
            return _value.IndexOf(item, index);
        }

        public int IndexOf(T item)
        {
            return _value.IndexOf(item);
        }

        public List<T> GetRange(int index, int count)
        {
            return _value.GetRange(index, count);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return _value.FindLastIndex(startIndex, count, match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return _value.FindLastIndex(startIndex, match);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return _value.FindLastIndex(match);
        }

        public T FindLast(Predicate<T> match)
        {
            return _value.FindLast(match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return _value.FindIndex(startIndex, count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return _value.FindIndex(startIndex, match);
        }

        public int FindIndex(Predicate<T> match)
        {
            return _value.FindIndex(match);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _value.CopyTo(array, arrayIndex);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            _value.CopyTo(index, array, arrayIndex, count);
        }

        public void CopyTo(T[] array)
        {
            _value.CopyTo(array);
        }

        public bool Contains(T item)
        {
            return _value.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _value.GetEnumerator();
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