using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Beobach.Observables;

namespace Beobach.BindingProviders
{
    public static class SimpleBindings
    {
        public static void BindText(this ObservableProperty<string> property, params Control[] controls)
        {
            property.Subscribe(value =>
            {
                foreach (var control in controls)
                {
                    control.Text = value;
                }
            }, controls);
            for (int i = 0; i < controls.Length; i++)
            {
                var control = controls[i];
                control.Text = property;
                if (!property.IsReadOnly)
                {
                    control.TextChanged += (sender, args) => property.Value = control.Text;
                }
            }
        }

        public static void BindText(this ObservableProperty<int> property, params Control[] controls)
        {
            property.Subscribe(value =>
            {
                foreach (var control in controls)
                {
                    control.Text = value.ToString();
                }
            }, controls);
            for (int i = 0; i < controls.Length; i++)
            {
                var control = controls[i];
                control.Text = property.Value.ToString();
                if (!property.IsReadOnly)
                {
                    control.TextChanged += (sender, args) => property.Value = Convert.ToInt32(control.Text);
                }
            }
        }

        public static void BindCheckBox(this ObservableProperty<bool> property, CheckBox checkBox)
        {
            property.Subscribe(value => checkBox.Checked = value, checkBox);
            checkBox.Checked = property;
            if (!property.IsReadOnly)
                checkBox.CheckedChanged += (sender, args) => property.Value = checkBox.Checked;
        }

        public static void BindDate(this ObservableProperty<DateTime> property, MonthCalendar calendar)
        {
            calendar.SetDate(property);
            calendar.DateSelected += (sender, args) => property.Value = calendar.SelectionStart;
            property.Subscribe(calendar.SetDate, calendar);

        }

        public static void BindSelectedComboBox<T>(this ObservableProperty<T> property, ComboBox comboBox)
        {
            comboBox.SelectedItem = property.Value;
            property.Subscribe(value => comboBox.SelectedItem = value, comboBox);
            comboBox.SelectedIndexChanged += (sender, args) => property.Value = (T) comboBox.SelectedItem;

        }
    }
}