using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beobach.Observables;

namespace Beobach
{
    public static class Observe
    {
        public static ObservableProperty<T> Value<T>(T value)
        {
            return new ObservableProperty<T>(value);
        }

        public static ObservableList<T> List<T>(List<T> list)
        {
            return new ObservableList<T>(list);
        }

        public static ObservableList<T> List<T>(IEnumerable<T> list)
        {
            return new ObservableList<T>(list);
        }

        public static ObservableList<T> List<T>(params T[] values)
        {
            return List(values.AsEnumerable());
        } 

        public static ComputedObservable<T> Compute<T>(ComputeCallBack<T> callBack)
        {
            return new ComputedObservable<T>(callBack);
        }

        public static ComputedObservable<T> TwoWayComputed<T>(ComputeCallBack<ObservableProperty<T>> callBack)
        {
            return new ComputedObservable<T>(() => callBack().Value, value => callBack().Value = value);
        } 
    }
}