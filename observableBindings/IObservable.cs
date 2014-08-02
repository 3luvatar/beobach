using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Fasterflect;

namespace observableBindings
{
    public interface IObservable
    {
        Dictionary<string, object> Values { get; }
        //Dictionary<Delegate, string> Delegates { get; }
        Dictionary<string, Delegate> PropertyNames { get; }
        Dictionary<Delegate, Delegate> Events { get; }
        ObservableProperty<T> GetObservableProperty<T>(string propertyName);
        Dictionary<int, Delegate> ComputedObservables { get; }
        void RegisterComputed<T>(ObservableProperty<T> computedProperty, string propertyName);
    }

    public delegate T ObservableProperty<T>(params T[] value);

    public delegate void ObservableEvent<T>(ObservableEventArgs<T> e);

    public static class ObservableExtensions
    {
        public static void Init<T>(this T observable) where T : IObservable
        {
            ObjectMap map = ObjectMap.GetMap(observable.GetType());
            foreach (
                var property in
                    map.Properties.Where(property => property.DestinationType.Inherits(typeof (ObservableProperty<>))))
            {
                Type backingType = property.DestinationType.GenericTypeArguments.Single();
                Delegate @delegate = GetObserver(observable, property, backingType);
                if (property.Set != null)//for observable values
                {
                    property.Set(observable, @delegate);
                }
                else//else computed observer
                {

                }
            }
        }

        public static Delegate GetObserver(IObservable observable, Property property, Type backingType)
        {
            Type staticType = typeof (ObservableExtensions);
            var convertedMethod =
                staticType.Method("GetObservableProperty", Flags.StaticPrivate).MakeGenericMethod(backingType);
            object @delegate = convertedMethod.Invoke(null, new object[] {observable, property});
            return (Delegate) @delegate;
        }

        private static ObservableProperty<T> GetObservableProperty<T>(IObservable observable, Property property)
        {
            string propertyName = property.PropertyName;
            ObservableProperty<T> retVal;
            if (property.Set == null)
            {
                retVal = (ObservableProperty<T>) property.Get(observable);
                observable.RegisterComputed(retVal, propertyName);
            }
            else
            {
                retVal = observable.GetObservableProperty<T>(propertyName);
            }
            observable.PropertyNames.Add(propertyName, retVal);
            return retVal;
        }

        public static T GetterSetter<T>(this IObservable observable, string propertyName, T[] values)
        {
            if (!observable.Values.ContainsKey(propertyName)) observable.Values.Add(propertyName, default(T));
            T retVal = Get<T>(observable, propertyName);

            if (values.Any())
            {
                T newVal = values.Single();
                var e = new ObservableEventArgs<T>(retVal, newVal);
                ObservableEvent<T> @event = observable.GetEvent<T>(propertyName);
                if (@event != null) @event(e);
                if (!e.Handled) Set(observable, propertyName, newVal);
            }
            return retVal;
        }

        public static ObservableEvent<T> GetEvent<T>(this IObservable observable, string propertyName)
        {
            return GetEvent(observable, (ObservableProperty<T>) observable.PropertyNames[propertyName]);
        }

        public static ObservableEvent<T> GetEvent<T>(this IObservable observable,
            ObservableProperty<T> observableProperty)
        {
            if (!observable.Events.ContainsKey(observableProperty)) return null;
            return (ObservableEvent<T>) observable.Events[observableProperty];
        }

        private static T Get<T>(IObservable observable, string propertyName)
        {
            return (T) observable.Values[propertyName];
        }

        private static void Set<T>(IObservable observable, string propertyName, T value)
        {
            observable.Values[propertyName] = value;
        }

        public static void Subscribe<T, T2>(this T observable,
            Func<T, ObservableProperty<T2>> observableProperty, ObservableEvent<T2> callback) where T : IObservable
        {
            ObservableProperty<T2> propertyDelegate = observableProperty(observable);
            Subscribe(observable, propertyDelegate, callback);
        }

        private static void Subscribe<T>(this IObservable observable,
            ObservableProperty<T> observableProperty, ObservableEvent<T> callback)
        {
            if (!observable.Events.ContainsKey(observableProperty))
            {
                observable.Events.Add(observableProperty, callback);
            }
            else
            {
                ObservableEvent<T> existingEvent = (ObservableEvent<T>) observable.Events[observableProperty];
                observable.Events[observableProperty] = existingEvent + callback;
            }
        }

        public static ObservableProperty<T_Return> RegisterComputed<T_Return, T, T2>(this IObservable observable, ObservableProperty<T> property1,
            ObservableProperty<T2> property2, Func<T, T2, T_Return> computeValue)
        {
            int computedHash = computeValue.GetHashCode();
            if (observable.ComputedObservables.ContainsKey(computedHash))
            {
                return (ObservableProperty<T_Return>) observable.ComputedObservables[computedHash];
            }
            //create the observable, note that any values passed in are not used
            //consider throwing an exception if value.Length > 0
            ObservableProperty<T_Return> computedObservable = value => computeValue(property1(), property2());

            observable.Subscribe(property1, args =>
            {//must call get even inside subscribe so as not to only call subscribers created before registration
                var computedEvent = observable.GetEvent(computedObservable);
                if (computedEvent != null)
                {
                    computedEvent(new ObservableEventArgs<T_Return>(computedObservable(),
                        computeValue(args.NewValue, property2())));
                }
            });

            observable.Subscribe(property2, args =>
            {
                var computedEvent = observable.GetEvent(computedObservable);
                if (computedEvent != null)
                {
                    computedEvent(new ObservableEventArgs<T_Return>(computedObservable(),
                        computeValue(property1(), args.NewValue)));
                }
            });

            observable.ComputedObservables.Add(computedHash, computedObservable);
            return computedObservable;
        }

        public static ObservableProperty<T_Return> RegisterComputed<T_Return, T, T2, T3>(this IObservable observable, ObservableProperty<T> property1,
    ObservableProperty<T2> property2, ObservableProperty<T3> property3, Func<T, T2, T3, T_Return> computeValue)
        {
            int computedHash = computeValue.GetHashCode();
            if (observable.ComputedObservables.ContainsKey(computedHash))
            {
                return (ObservableProperty<T_Return>)observable.ComputedObservables[computedHash];
            }
            //create the observable, note that any values passed in are not used
            //consider throwing an exception if value.Length > 0
            ObservableProperty<T_Return> computedObservable = value => computeValue(property1(), property2(), property3());

            observable.Subscribe(property1, args =>
            {//must call get even inside subscribe so as not to only call subscribers created before registration
                var computedEvent = observable.GetEvent(computedObservable);
                if (computedEvent != null)
                {
                    computedEvent(new ObservableEventArgs<T_Return>(computedObservable(),
                        computeValue(args.NewValue, property2(), property3())));
                }
            });

            observable.Subscribe(property2, args =>
            {
                var computedEvent = observable.GetEvent(computedObservable);
                if (computedEvent != null)
                {
                    computedEvent(new ObservableEventArgs<T_Return>(computedObservable(),
                        computeValue(property1(), args.NewValue, property3())));
                }
            });

            observable.Subscribe(property3, args =>
            {
                var computedEvent = observable.GetEvent(computedObservable);
                if (computedEvent != null)
                {
                    computedEvent(new ObservableEventArgs<T_Return>(computedObservable(),
                        computeValue(property1(), property2(), args.NewValue)));
                }
            });


            observable.ComputedObservables.Add(computedHash, computedObservable);
            return computedObservable;
        }

//        public static ObservableProperty<T_Return> RegisterComputed<T_Return, T>(this T observable,
//            Func<T_Return> compute, params Delegate[] dependantProperties) where T : IObservable
//        {
//            int computedHash = compute.GetHashCode();
//            if (observable.ComputedObservables.ContainsKey(computedHash))
//            {
//                return (ObservableProperty<T_Return>) observable.ComputedObservables[computedHash];
//            }
//            if (dependantProperties.Any(@delegate => !@delegate.GetType().Inherits(typeof (ObservableProperty<>))))
//            {
//                throw new Exception("Error invalid delegate passed in RegisterComputed");
//            }
//            ObservableProperty<T_Return> propertyDelegate = value => compute();
//            Action<T_Return> getAndCallEvent = (o) =>
//            {
//                var events = observable.GetEvent(propertyDelegate);
//                if (events != null)
//                {
//                    events(new ObservableEventArgs<T_Return>(propertyDelegate(), o));
//                }
//            };
//
//            foreach (Delegate dependantProperty in dependantProperties)
//            {
//                
//            }
//
//            return propertyDelegate;
//        }
//
//        private static void Subscribe(IObservable observable, Delegate dependant, Action callBack)
//        {
//            
//        }
//
//        private static Delegate MakeEventForComputed<T>(IObservable observable, Action<T> callback,  Delegate dependant)
//        {
//            Type dependantBackingType = dependant.GetType().GetGenericArguments().Single();
//            Type staticType = typeof(ObservableExtensions);
//            var convertedMethod =
//                staticType.Method("Subscribe", new []{typeof(IObservable), typeof(ObservableProperty<>), typeof(ObservableEvent<>)}, Flags.StaticPrivate).MakeGenericMethod(dependantBackingType);
//            object @delegate = convertedMethod.Invoke(null, new object[] { observable, propertyName });
//            return (Delegate)@delegate;
//        }
//
//        private static void MakeEventComputer<T>(IObservable observable, )
    }
}