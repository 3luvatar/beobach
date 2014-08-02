using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace observableBindings
{
    public class ObservableEventArgs<T>
    {
        public ObservableEventArgs(T oldValue, T newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        private T _oldValue;
        private T _newValue;
        private bool _handled;

        public T OldValue
        {
            get { return _oldValue; }
        }

        public T NewValue
        {
            get { return _newValue; }
        }

        public bool Handled
        {
            get { return _handled; }
            set { _handled = value; }
        }
    }
}