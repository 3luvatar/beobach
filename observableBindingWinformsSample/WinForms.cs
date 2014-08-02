using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Beobach.BindingProviders
{
    public static class WinForms
    {
        public static void BindText(this ObservableProperty<string> property, params Control[] controls)
        {
            property.Subscribe(value =>
            {
                foreach (var control in controls)
                {
                    control.Text = value;
                }
            });
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

        public static void BindCheckBox(this ObservableProperty<bool> property, CheckBox checkBox)
        {
            property.Subscribe(value => checkBox.Checked = value);
            if (!property.IsReadOnly)
                checkBox.CheckedChanged += (sender, args) => property.Value = checkBox.Checked;
            checkBox.Checked = property;
        }
    }
}