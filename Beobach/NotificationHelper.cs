using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beobach.Observables;

namespace Beobach
{
    public static class NotificationHelper
    {
        private static event Action<IObservableProperty> OnValueAccessed;
        private static event Action<IObservableList, int> OnIndexAccessed;

        internal static HashSet<PropertyAccessNotification> CatchValuesAccessed(Action access)
        {
            var accessNotifications = new HashSet<PropertyAccessNotification>();
            Action<IObservableProperty> accessed = property => accessNotifications.Add(new PropertyAccessNotification(property));
            Action<IObservableList, int> accessedIndex =
                (list, i) => accessNotifications.Add(new IndexAccessNotification(list, i));
            OnValueAccessed += accessed;
            OnIndexAccessed += accessedIndex;
            access();
            OnValueAccessed -= accessed;
            OnIndexAccessed -= accessedIndex;
            return accessNotifications;
        }

        internal static void ValueAccessed(IObservableProperty observableProperty)
        {
            observableProperty.IsAccessed = true;
            Action<IObservableProperty> handler = OnValueAccessed;
            if (handler != null) handler(observableProperty);
            observableProperty.IsAccessed = false;
        }

        internal static void IndexAccessed(IObservableList observableList, int index)
        {
            observableList.IsAccessed = true;
            var handler = OnIndexAccessed;
            if (handler != null) handler(observableList, index);
            observableList.IsAccessed = false;
        }

        internal static event Action<object> OnNotifyAccessed;

        internal static List<object> CatchNotifications(Action notify)
        {
            var subscribers = new List<object>();
            Action<object> notified = subscribers.Add;
            OnNotifyAccessed += notified;
            notify();
            OnNotifyAccessed -= notified;
            return subscribers;
        }

        internal static void NotificationSent(object subscriber)
        {
            if (OnNotifyAccessed != null)
            {
                OnNotifyAccessed(subscriber);
            }
        }
    }
}