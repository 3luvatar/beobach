using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beobach;

namespace observableBindingWinformsSample
{
    public class SampleViewModel
    {
        public readonly ObservableProperty<string> FirstName = new ObservableProperty<string>();
        public readonly ObservableProperty<string> LastName = new ObservableProperty<string>();
        public readonly ObservableProperty<string> MiddleName = new ObservableProperty<string>();
        public readonly ObservableProperty<bool> HasMiddleName = new ObservableProperty<bool>(); 
        public readonly ComputedObservable<string> FullName;


        public SampleViewModel()
        {
            FullName =
                new ComputedObservable<string>(
                    () => string.Format("{0}{2} {1}", FirstName.Value, LastName.Value, HasMiddleName ? (" " + MiddleName.Value) : ""));
        }
    }
}