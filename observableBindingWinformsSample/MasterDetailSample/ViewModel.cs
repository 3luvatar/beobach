using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beobach.Observables;

namespace observableBindingWinformsSample.MasterDetailSample
{
    public class ViewModel
    {
        public readonly ObservableList<Guest> Guests =
            new ObservableList<Guest>(new Guest("Becky", new DateTime(2014, 7, 25)) {FoodChoice = { Value = FoodChoice.bacon}},
                new Guest("Jonathan", new DateTime(2014, 8, 1)) {FoodChoice = { Value = FoodChoice.steak}},
                new Guest("Jacob", new DateTime(2014, 8, 5)) {FoodChoice = { Value = FoodChoice.chicken}});

        public readonly ComputedObservable<int> AttendingGuests;
        public readonly ComputedObservable<Dictionary<FoodChoice, int>> FoodTypeCounts;
        public readonly ObservableProperty<Guest> SelectedGuest = new ObservableProperty<Guest>();

        public ViewModel()
        {
//            FoodTypeCounts =
//                new ComputedObservable<Dictionary<FoodChoice, int>>(
//                    () =>
//                        Guests.GroupBy(guest => guest.FoodChoice.Value,
//                            (choice, guests) => new {choice, count = guests.Count()})
//                            .ToDictionary(arg => arg.choice, arg => arg.count));
            AttendingGuests = new ComputedObservable<int>(() => Guests.Count(guest => guest.Attending));
            SelectedGuest.Value = Guests.First();
        }

        public void AddNew()
        {
            Guests.Add(new Guest("", DateTime.Now));
            SelectedGuest.Value = Guests.Last();
        }
    }

    public class Guest
    {
        public readonly ObservableProperty<bool> Attending = new ObservableProperty<bool>();
        public readonly ObservableProperty<string> Name = new ObservableProperty<string>();
        public readonly ObservableProperty<FoodChoice> FoodChoice = new ObservableProperty<FoodChoice>();
        public readonly ObservableProperty<DateTime> InvitedDate = new ObservableProperty<DateTime>();

        public Guest(string name, DateTime invitedDate)
        {
            Name.Value = name;
            InvitedDate.Value = invitedDate;
        }
    }

    public enum FoodChoice
    {
        bacon,
        chicken,
        steak,
        iceCream
    }
}