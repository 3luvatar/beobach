using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using observableBindings;

namespace observableBindingWpfSample
{
    public class TestObject : Observable
    {
        public ObservableProperty<string> FirstName { get; private set; }
        public ObservableProperty<string> LastName { get; private set; }
        public ObservableProperty<string> MiddleName { get; private set; }

        public ObservableProperty<string> FullName
        {
            get { return RegisterComputed(FirstName, MiddleName, LastName, (s, s1, s2) => string.Format("Name: {0} {1} {2}", s, s1, s2)); }
        }

        public TestObject()
        {
            OnButtonClick = new RelayCommand(o => FirstName(string.Concat(FirstName(), FirstName())));
        }

        public ICommand OnButtonClick { get; set; }
    }
}