using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Beobach;
using Beobach.Observables;
using Beobach.Subscriptions;

namespace observableBindingWinformsSample.Bindings
{
    public static class ListBinding
    {
        public static ListBinding<T_MODEL> Bind<T_MODEL>(this ObservableList<T_MODEL> list,
            ListBox listBox,
            Func<T_MODEL, ObservableProperty<string>> getText,
            ObservableProperty<T_MODEL> selectedItem) where T_MODEL : class
        {
            return new ListBinding<T_MODEL>(list, listBox, getText, selectedItem);
        }
    }

    public class ListBinding<T_MODEL> where T_MODEL : class
    {
        private readonly ObservableList<T_MODEL> _list;
        private readonly ListBox _listBox;
        private readonly Func<T_MODEL, ObservableProperty<string>> _getText;
        private readonly ObservableProperty<T_MODEL> _selectedItem;
        private bool _supressIndexChange;

        public ListBinding(ObservableList<T_MODEL> list,
            ListBox listBox,
            Func<T_MODEL, ObservableProperty<string>> getText,
            ObservableProperty<T_MODEL> selectedItem)
        {
            _supressIndexChange = false;
            _list = list;
            _listBox = listBox;
            _getText = getText;
            _selectedItem = selectedItem;
//            items = Observe.Compute(() => list.Select(model => new ListItem(_listBox, getText(model))).ToArray());
            _listBox.DataSource = _list.Select(model => _getText(model).Value).ToArray();
            _listBox.SelectedIndex = _list.IndexOf(selectedItem.Value);
            foreach (var model in _list)
            {
                _getText(model)
                    .Subscribe(
                        value => UpdateList(),
                        _listBox);
            }
            _list.SubscribeArrayChange(value =>
            {
                foreach (var arrayChange in value)
                {
                    if (arrayChange.ChangeType == ArrayChangeType.add)
                    {
                        _getText(
                            arrayChange.Value)
                            .Subscribe(
                                text => UpdateList(),
                                _listBox);
                    }
                    else
                    {
                        //TODO dispose any subscriptions
                    }
                }

                UpdateList();
            },
                _listBox);
            _selectedItem.Subscribe(value => _listBox.SelectedIndex = _list.IndexOf(value), this);
            _listBox.SelectedIndexChanged += (sender, args) =>
            {
                if (_supressIndexChange) return;
                _selectedItem.Value = _listBox.SelectedIndex == -1
                    ? null
                    : _list[_listBox.SelectedIndex];
            };
        }

        private void UpdateList()
        {
            _supressIndexChange = true;
            _listBox.DataSource = _list.Select(model => _getText(model).Value).ToArray();
            _listBox.SelectedIndex = _list.IndexOf(_selectedItem.Value);
            _supressIndexChange = false;
        }

        public class ListItem
        {
            private ObservableProperty<string> name;

            public ListItem(ListBox listBox, ObservableProperty<string> name)
            {
//                this.name = name;
            }

            public override string ToString()
            {
                return name.Value;
            }
        }
    }
}