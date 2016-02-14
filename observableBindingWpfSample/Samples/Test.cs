using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Beobach.Observables;

namespace observableBindingWpfSample.Samples
{
    public class Test : DependencyObject
    {
        public readonly ObservableProperty<string> FirstName = new ObservableProperty<string>();
        public readonly ObservableProperty<string> LastName = new ObservableProperty<string>();
        public readonly ObservableProperty<string> MiddleName = new ObservableProperty<string>();
        public readonly ObservableProperty<bool> HasMiddleName = new ObservableProperty<bool>();
    }
}
