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
        internal static event Action<IObservableProperty> OnValueAccessed;

        internal static HashSet<IObservableProperty> CatchValuesAccessed(Action access)
        {
            var properties = new HashSet<IObservableProperty>();
            Action<IObservableProperty> accessed = property => properties.Add(property);
            OnValueAccessed += accessed;
            access();
            OnValueAccessed -= accessed;
            return properties;
        }

        internal static void ValueAccessed(IObservableProperty observableProperty)
        {
            observableProperty.IsAccessed = true;
            Action<IObservableProperty> handler = OnValueAccessed;
            if (handler != null) handler(observableProperty);
            observableProperty.IsAccessed = false;
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