using Beobach.Observables;
using observableBindingWpfSample.ViewModelBase;

namespace observableBindingWpfSample.Samples
{
    public class SimpleViewModel : ViewModel
    {
        public readonly ObservableProperty<string> FirstName = new ObservableProperty<string>();
        public readonly ObservableProperty<string> LastName = new ObservableProperty<string>();
        public readonly ObservableProperty<string> MiddleName = new ObservableProperty<string>();
        public readonly ObservableProperty<bool> HasMiddleName = new ObservableProperty<bool>();
        public readonly ComputedObservable<string> FullName;

        public SimpleViewModel()
        {
            FullName =
                new ComputedObservable<string>(
                    () =>
                        string.Format("{0}{2} {1}",
                            FirstName.Value,
                            LastName.Value,
                            HasMiddleName ? (" " + MiddleName.Value) : ""));
        }
    }
}