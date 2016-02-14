using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Beobach.Observables;
using Fasterflect;
using observableBindingWpfSample.Annotations;

namespace observableBindingWpfSample.ViewModelBase
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected List<IObservableProperty> ObservableProperties = new List<IObservableProperty>();

        public ViewModel()
        {
            foreach (var fieldInfo in GetType().Fields().Where(info => info.FieldType.Implements<IObservableProperty>()))
            {
                Type observableType = fieldInfo.FieldType;
                var observable = (IObservableProperty) fieldInfo.Get(this);
                if (observable == null)
                {
                    if (observableType.Inherits(typeof(ComputedObservable<>))) throw new NotSupportedException("computed observables must be defined ahead of time, field:" + fieldInfo.Name + " was not defined");
                    observable = (IObservableProperty) observableType.CreateInstance();
                    fieldInfo.Set(this, observable);
                }
                string fieldName = fieldInfo.Name;
                ObservableAdded(observable, fieldName);
            }
        }

        protected void ObservableAdded(IObservableProperty observable, string fieldName)
        {
            observable.Subscribe(o => OnPropertyChanged(fieldName), this);
            ObservableProperties.Add(observable);
        }
    }
}