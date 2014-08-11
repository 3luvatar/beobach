using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Beobach;
using Beobach.BindingProviders;
using Beobach.Observables;

namespace observableBindingWinformsSample.MasterDetailSample
{
    public partial class MasterControl : UserControl
    {
        public MasterControl()
        {
            InitializeComponent();
            foodTypeComboBox.Items.AddRange(Enum.GetValues(typeof (FoodChoice)).OfType<object>().ToArray());
        }

        private ComputedObservable<string> _name;
        private ComputedObservable<bool> _attending;
        private ComputedObservable<DateTime> _invited;
        private ComputedObservable<FoodChoice> _foodChoice;
        private ObservableProperty<Guest> _guest;

        public void View(ObservableProperty<Guest> selectedGuest)
        {
            if (_guest != null) throw new InvalidOperationException("guest already setup");
            _guest = selectedGuest;
            _name = Observe.TwoWayComputed(() => _guest.Value.Name);
            _attending = Observe.TwoWayComputed(() => _guest.Value.Attending);
            _invited = Observe.TwoWayComputed(() => _guest.Value.InvitedDate);
            _foodChoice = Observe.TwoWayComputed(() => _guest.Value.FoodChoice);
            _name.BindText(nameTextBox);
            _attending.BindCheckBox(attendingCheckBox);
            _invited.BindDate(invitedDateCalendar);
            _foodChoice.BindSelectedComboBox(foodTypeComboBox);
        }
    }
}