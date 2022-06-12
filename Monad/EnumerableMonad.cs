
// #define UseYieldReturn


using System;
using System.Collections;
using System.Collections.Generic;





namespace Monad.Enumerable
{

    // extension methods for the EnumerableMonad

    public static class ListMonadExtensionMethods
    {

#if !UseYieldReturn

        public static IEnumerable<U> Bind<T, U>(this IEnumerable<T> source, Func<T, IEnumerable<U>> function)
        {
            return new EnumerableMonad<T, U>(source, function);
        }

#else

        // Bind would normall be implemented like this, but since the yield return causes the complier to generate the actual code that does the work
        // and our purpose here is to show how Bind works, we provide the version above which uses a class which is baically what the comiler would have generated when it saw the yield return keywords.
        public static IEnumerable<U> Bind<T, U>(this IEnumerable<T> source, Func<T, IEnumerable<U>> function)
        {
            foreach (var t in source)
            {
                var enumerator = function(t);
                foreach (var u in enumerator)
                {
                    yield return u;
                }
            }
        }

#endif

    }




    // This is the class that will be used by Bind to implement the monad.
    // A class is needed becasue the monad is deferred, so this class suports the structure that will be built.
    // Bind will pass it the source IEnumerable and the application function in the constructor.
    // Note that this monad is a deferred/pull type of monad. 
    // The class is basically driven by the MoveNext method, which is called by the next monad down the chain.
    // MoveNext does everything.
    // The class is completely lazy, so it doesn't even get the source IEnumerator from the source IEnumerable until the first call of MoveNext.

    // Although this class has nothing to do with ALA (it is solely to support the monad version), note how similar it is to an ALA domain abstraction class.
    // It has an input "port" which is the source field, which is wired by the Bind function via the constructor.
    // It has an output "port", which is the implemented IEnumerable interface, which is wired by the next Bind function.
    // Note that these are wired in the opposite direction to the dataflow. Tht's becasue this is a pull type monad
    // Indeed the ALA version is based on this class. but will be wired up using the WireIn operator instead.



    class EnumerableMonad<T, U> : IEnumerator<U>, IEnumerable<U>
    {
        //------------------------------------------------------------------------
        // implement the constructor


        private readonly IEnumerable<T> source;
        private readonly Func<T, IEnumerable<U>> function;
        public EnumerableMonad(IEnumerable<T> source, Func<T, IEnumerable<U>> function) { this.source = source; this.function = function; }




        //------------------------------------------------------------------------
        // Implement the IEnumerable interface

        IEnumerator<U> IEnumerable<U>.GetEnumerator()
        {
            return (IEnumerator<U>)this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }




        //------------------------------------------------------------------------
        // Implement the IEnumertor interface


        // These two fields define our state
        // The first one can have a state of null, which is the state before we got the first value, otherwise it holds the state as we go through the source IEnumerator
        private IEnumerator<T> sourceEnumerator = null;    // the IEnumerator from the source (left side) of the bind
        
        // This holds the state as we go through IEnumerator returned from the function
        private IEnumerator<U> functionEnumerator = null;  // The current IEnumerator returned from the function


        U IEnumerator<U>.Current => functionEnumerator.Current;   // If the output has already used MoveNext and returned true, there wil always be a current value sitting in the functionEnumerator

        object IEnumerator.Current => throw new NotImplementedException();

        void IDisposable.Dispose() { }

        bool IEnumerator.MoveNext()
        {
            // Need a while loop to get past empty lists returned by the function, and also when there are no more values
            // to be had from the function IEnumerator, it needs to get a new IEnumerator from the function, and loop around.
            while (true)
            {
                // If there is an IEnumerator from the function already in place, just use it.
                if (functionEnumerator != null)
                {
                    if (functionEnumerator.MoveNext())
                    {
                        return true;
                    }
                    // no values left in the current IEnumertor from the function
                }
                // There is no IEnumerator from the function (at the beginning), or it is exhausted
                if (sourceEnumerator == null) sourceEnumerator = source.GetEnumerator();  // At the very start we need to get the IEnumerator from the source. This hapns only once.
                if (sourceEnumerator.MoveNext())
                {
                    functionEnumerator = function(sourceEnumerator.Current).GetEnumerator();
                }
                else
                {
                    return false;  // finished going through the IEnumerator from the source, so we are completely finished
                }
            }
        }

        void IEnumerator.Reset()
        {
            functionEnumerator = null;
            sourceEnumerator = null;  
        }
    }
}
