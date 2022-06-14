using System;
using System.Collections.Generic;

namespace Monad.List
{
    // --------------------------------------------------------------------------------------------
    // List monad.
    // This is a convnetional list monad, its convnetional by using a pull interface, and by working immediately (not deferred). In other words the Bind evalutaes the composed application functions as it goes.  
    // The main interface is List<T>.
    // The Bind method operates on a List<T> and returns a List<U>.
    // The composed function must take a T and return a List<T> 

    public static class MonadExtensionMethods
    {
        public static List<U> Bind<T, U>(this List<T> source, Func<T, List<U>> function)
        {
            List<U> output = new List<U>();
            foreach (T t in source)
            {
                output.AddRange(function(t));
            }
            return output;
        }
    }
}
