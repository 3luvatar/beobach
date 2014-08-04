using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beobach.Observables;

namespace observableBindingWinformsSample
{
    public class CheckBoxSampleViewModel
    {
        public ObservableList<InvitedGuestViewModel> InvitedGuests { get; private set; }
        public ComputedObservable<int> AttendingGuests { get; private set; }

        public CheckBoxSampleViewModel()
        {
            InvitedGuests = new ObservableList<InvitedGuestViewModel>(new InvitedGuestViewModel("Ted"),
                new InvitedGuestViewModel("Fred"),
                new InvitedGuestViewModel("Becky", true),
                new InvitedGuestViewModel("Nathan"));
            AttendingGuests = new ComputedObservable<int>(() => InvitedGuests.Count(model => model.Attending));
        }

        public void Invite(string name)
        {
            InvitedGuests.Add(new InvitedGuestViewModel(name));
        }
    }

    public class InvitedGuestViewModel
    {
        public ObservableProperty<bool> Attending { get; private set; }
        public ObservableProperty<string> Name { get; private set; }

        public InvitedGuestViewModel(string name, bool attending = false)
        {
            Attending = new ObservableProperty<bool>(attending);
            Name = new ObservableProperty<string>(name);
        }
    }
}