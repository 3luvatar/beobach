using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Beobach.Observables;

namespace observableBindingWinformsSample.Bindings
{
    public static class CheckBoxListBinding
    {
        public static CheckBoxBinding<T_MODEL> Bind<T_MODEL>(this
            ObservableList<T_MODEL> observableList,
            CheckedListBox listBox,
            Func<T_MODEL, ObservableProperty<string>> getText,
            Func<T_MODEL, ObservableProperty<bool>> getChecked)
        {
            return new CheckBoxBinding<T_MODEL>(listBox, observableList, getText, getChecked);
        }
    }

    public class CheckBoxBinding<T_MODEL>
    {
        private CheckedListBox ListBox;
        private Func<T_MODEL, ObservableProperty<bool>> getChecked;
        private Func<T_MODEL, ObservableProperty<string>> getText;
        private ObservableList<T_MODEL> ObservableList;
        private ComputedObservable<ItemModel[]> itemModels;

        public CheckBoxBinding(CheckedListBox listBox,
            ObservableList<T_MODEL> observableList,
            Func<T_MODEL, ObservableProperty<string>> getText,
            Func<T_MODEL, ObservableProperty<bool>> getChecked)
        {
            ListBox = listBox;
            ObservableList = observableList;
            this.getText = getText;
            this.getChecked = getChecked;
            itemModels =
                new ComputedObservable<ItemModel[]>(
                    () =>
                        ObservableList.Select((model, i) => new ItemModel(getText(model), getChecked(model), i))
                            .ToArray());
            itemModels.Subscribe(value =>
            {
                ListBox.Items.Clear();
                ListBox.Items.AddRange(itemModels.Value);
                updateChecked();
            },
                this);
            ListBox.Items.AddRange(itemModels.Value);
            updateChecked();
            ListBox.ItemCheck +=
                (sender, e) => {
                    {
                        itemModels.Value[e.Index].IsChecked.Value = e.NewValue == CheckState.Checked;
                    } };
        }

        private void updateChecked()
        {
            {
                for (int i = 0; i < itemModels.Value.Length; i++)
                {
                    ItemModel itemModel = itemModels.Value[i];
                    itemModel.IsChecked.Subscribe(value =>
                    {
                        if (ListBox.GetItemChecked(itemModel.Index) == value) return;
                        ListBox.SetItemChecked(itemModel.Index, value);
                    }, this);
                    ListBox.SetItemChecked(i, itemModel.IsChecked);
                    itemModel.Text.Subscribe(value => ListBox.Invalidate(), this);
                }
            }
        }

        private class
            ItemModel
        {
            public
                ItemModel(ObservableProperty<string> text, ObservableProperty<bool> isChecked, int index)
            {
                this.Text = text;
                this.IsChecked = isChecked;
                Index = index;
            }

            internal
                ObservableProperty<bool>
                IsChecked { get; set; }

            public int Index { get; set; }

            internal
                ObservableProperty<string>
                Text { get; set; }

            public override
                string ToString
                ()
            {
                return Text;
            }
        }
    }
}