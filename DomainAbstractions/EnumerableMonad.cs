using Foundation;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DomainAbstractions
{
    // extension methods for the EnumerableMonad

    public static class ListMonadExtensionMethods
    {
        // This Bind function is identical to the one in the Monad namespace implementation
        
        public static IEnumerable<U> Bind<T, U>(this IEnumerable<T> source, Func<T, IEnumerable<U>> function)
        {
            var em = new EnumerableMonad<T, U>(function);
            source.WireToR(em);
            return em;
        }
    }




    // This is the ALA domain absraction class to support monad behaviour - composing functions that take a value and return an IEnumerable.
    // It is configured with a lambda function from the application layer.
    // It has an input port and an output port which both use the iEnumerable programming paradigm
    // It is Wireable using WireInR.
    // WirInR is used like WireIn, but does the actual wiring in reverse which is required by the IEnumerable interface.
    // source.WireIn(new EnumerableMonad(lambda)) 
    // Bind is only provided to make the syntax of its use exactly the same as monads. We wouldn't normally provide it. We would use WireInR instead
    // .Bind(lambda)
    // Note that the body of this class is identical to the one used for the monad implementaion in the monad namespace.
    // Note that this monad is a deferred/pull type of monad. 
    // The class is basically driven by the MoveNext method, which is called by the next monad down the chain.
    // MoveNext does everything.
    // The class is completely deferred, so it doesn't even get the source IEnumerator from the source IEnumerable until the first call of MoveNext.

    // It has an input "port" which is the source IEnumrable field.
    // It has an output "port", which is the implemented IEnumerable interface.

    class EnumerableMonad<T, U> : IEnumerator<U>, IEnumerable<U>
    {
        //------------------------------------------------------------------------
        // implement the constructor


 
        private readonly Func<T, IEnumerable<U>> function;
        public EnumerableMonad(Func<T, IEnumerable<U>> function) { this.function = function; }


        private IEnumerable<T> source;  // main input port. This port is not wired directly by WireTo, but indirectly by the use of the WireForward interface.


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
