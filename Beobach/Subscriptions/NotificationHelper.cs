using System;
using System.Collections.Generic;
using Beobach.Observables;

namespace Beobach.Subscriptions
{
    public static class NotificationHelper
    {
        private static readonly Stack<Action<PropertyAccessNotification>> BeingComputed =
            new Stack<Action<PropertyAccessNotification>>();

        internal static HashSet<PropertyAccessNotification> CatchValuesAccessed(Action access)
        {
            var accessNotifications = new HashSet<PropertyAccessNotification>();
            BeingComputed.Push(notification => accessNotifications.Add(notification));
            access();
            BeingComputed.Pop();
            return accessNotifications;
        }

        internal static void ValueAccessed(IObservableProperty observableProperty)
        {
            observableProperty.IsAccessed = true;
            if (BeingComputed.Count > 0) BeingComputed.Peek()(new PropertyAccessNotification(observableProperty));
            observableProperty.IsAccessed = false;
        }

        internal static void IndexAccessed(IObservableList observableList, int index)
        {
            observableList.IsAccessed = true;
            if (BeingComputed.Count > 0) BeingComputed.Peek()(new IndexAccessNotification(observableList, index));

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