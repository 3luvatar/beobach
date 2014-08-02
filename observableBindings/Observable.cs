using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace observableBindings
{
    public class Observable : DependencyObject, IObservable
    {
        public Observable()
        {
            this.Init();
        }

        public static Dictionary<Type, Dictionary<string, DependencyProperty>> DependencyProperties =
            new Dictionary<Type, Dictionary<string, DependencyProperty>>();

        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        private readonly Dictionary<Delegate, string> _delegates = new Dictionary<Delegate, string>();
        private readonly Dictionary<string, Delegate> _propertyNames = new Dictionary<string, Delegate>();
        private readonly Dictionary<Delegate, Delegate> _events = new Dictionary<Delegate, Delegate>();
        private readonly Dictionary<int, Delegate> _computedObservables = new Dictionary<int, Delegate>();

        public Dictionary<string, Delegate> PropertyNames
        {
            get { return _propertyNames; }
        }

        public Dictionary<string, object> Values
        {
            get { return _values; }
        }

        public Dictionary<Delegate, string> Delegates
        {
            get { return _delegates; }
        }

        public Dictionary<Delegate, Delegate> Events
        {
            get { return _events; }
        }

        public Dictionary<int, Delegate> ComputedObservables
        {
            get { return _computedObservables; }
        }

        public ObservableProperty<T> GetObservableProperty<T>(string propertyName)
        {
            var propertyDelegate = (ObservableProperty<T>) (value => this.GetterSetter(propertyName, value));
            RegisterComputed(propertyDelegate, propertyName);
            return propertyDelegate;
        }

        public ObservableProperty<T_Return> RegisterComputed<T_Return, T, T2>(ObservableProperty<T> property1,
            ObservableProperty<T2> property2, Func<T, T2, T_Return> computeValue)
        {
            return ObservableExtensions.RegisterComputed(this, property1, property2, computeValue);
        }

        public ObservableProperty<T_Return> RegisterComputed<T_Return, T, T2, T3>(
            ObservableProperty<T> property1,
            ObservableProperty<T2> property2, ObservableProperty<T3> property3, Func<T, T2, T3, T_Return> computeValue)
        {
            return ObservableExtensions.RegisterComputed(this, property1, property2, property3, computeValue);
        }

        public void RegisterComputed<T>(ObservableProperty<T> computedProperty, string propertyName)
        {
            if (!DependencyProperties.ContainsKey(GetType())) //build dictonary for this type
            {
                DependencyProperties.Add(GetType(), new Dictionary<string, DependencyProperty>());
            }
            Dictionary<string, DependencyProperty> thisTypeProperties = DependencyProperties[GetType()];
            if (!thisTypeProperties.ContainsKey(propertyName)) //make dependency property for this type
            {
                thisTypeProperties.Add(propertyName, MakeDependencyProperty<T>(propertyName, GetType()));
            }

            ObservableEvent<T> onValueChangedEvent = args => SetValue(thisTypeProperties[propertyName], args.NewValue);
            if (!Events.ContainsKey(computedProperty))
            {
                Events.Add(computedProperty, onValueChangedEvent);
            }
            else
            {
                ObservableEvent<T> existingEvent = (ObservableEvent<T>) Events[computedProperty];
                Events[computedProperty] = existingEvent + onValueChangedEvent;
            }
        }

        private static DependencyProperty MakeDependencyProperty<T>(string propertyName, Type type)
        {
            return DependencyProperty.Register(propertyName, typeof (T), type,
                new PropertyMetadata((o, args) =>
                {
                    Observable @this = (Observable) o;
                    var observableProperty = @this.getObservableProperty<T>(propertyName);
                    observableProperty((T) args.NewValue);
                }));
        }

        private ObservableProperty<T> getObservableProperty<T>(string propertyName)
        {
            return (ObservableProperty<T>) PropertyNames[propertyName];
        }
    }
}