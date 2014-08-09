using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beobach;
using Beobach.Observables;

namespace observableBindingWinformsSample
{
    public class SelectedListViewModel
    {
        public ObservableList<Friend> Friends;

        public ObservableProperty<Friend> SelectedFriend;
        public ComputedObservable<string> SelectedFriendName;

        public SelectedListViewModel()
        {
            Friends = Observe.List(new Friend("jo"),
                new Friend("Jason"),
                new Friend("Rachael"),
                new Friend("Nathan"),
                new Friend("Bob"));
            SelectedFriend = new ObservableProperty<Friend>();
            SelectedFriendName =
                new ComputedObservable<string>(
                    () => SelectedFriend.Value == null ? string.Empty : SelectedFriend.Value.Name.Value,
                    value => SelectedFriend.Value.Name.Value = value);
        }

        public void AddFriend(string name)
        {
            Friends.Add(new Friend(name));
        }
    }

    public class Friend
    {
        public Friend(string name)
        {
            this.Name = Observe.Value(name);
        }

        public ObservableProperty<string> Name { get; set; }
    }
}