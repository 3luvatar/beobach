using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beobach
{
   public static class BeobachHelper
   {
       internal static event Action<IObservableProperty> OnValueAccessed;

       internal static void ValueAccessed(IObservableProperty observableProperty)
       {
           observableProperty.IsAccessed = true;
           Action<IObservableProperty> handler = OnValueAccessed;
           if (handler != null) handler(observableProperty);
           observableProperty.IsAccessed = false;
       }
    }
}
