


using ProgrammingParadigms;
using Foundation;
using System;
using System.Collections;
using System.Collections.Generic;





namespace DomainAbstractions
{

    // The way the IEnumerable ports work on this domain abstraction, you would need to use WireTo in the opposite direction to the dataflow, i.e. destination.WireTo(source).
    // That would work bu it would be utterly confusing.
    // This interface exists solely to provide ports for which WireTo works in the same direction as the dataflow.
    // That is the source has a field of this interface and the destination implements this interface
    // These ports will be wired, and immediately the source will call the Push method, which effectively gets the wiring done in the opposite direction, which is needed because its a pulling dataflow programming paradigm.
    public interface IWireable 
    {
        void Push(IEnumerable ie);
    }





    // extension methods for the EnumerableMonad

    public static class ListMonadExtensionMethods
    {
        // This Bind function is identical to the one in the Monad namespace implementation
        
        public static IEnumerable<U> Bind<T, U>(this IEnumerable<T> source, Func<T, IEnumerable<U>> function)
        {
            var em = new EnumerableMonad<T, U>(function);
            source.WireTo(em);
            return em;
        }

        public static IEnumerable<T> ToWireableEnumerable<T>(this IEnumerable<T> source)
        {
            return new ToEnumerableMonad<T>(source);
        }
    }




    // This is the ALA domain absraction class to support monad behaviour.
    // It is configured with a lambda function from the application layer
    // It is Wireable using WireIn or WireTo.
    // What's more, it is wireable in the same direction as the dataflow.
    // It can be used without Bind e.g. source.WireIn(new EnumerableMonad(lambda expressions))
    // Bind is only provided to make the syntax of its use exactly the same as monads. We wouldn't normally provide it. We would use .WireIn(new EnumerableMonad(lambda expressions)) instead.

    // Note that the body of this class is identical to the one used for the monad implementaion in the monad namespace.
    // Note that this monad is a deferred/pull type of monad. 
    // The class is basically driven by the MoveNext method, which is called by the next monad down the chain.
    // MoveNext does everything.
    // The class is completely deferred, so it doesn't even get the source IEnumerator from the source IEnumerable until the first call of MoveNext.

    // It has an input "port" which is the source field.
    // It has an output "port", which is the implemented IEnumerable interface.
    // Note that we don't want to do the wiring using these ports because we would have to wire in the opposite direction to the dataflow, which would be confusing.
    // So we use another pair of ports that go in the opposite direction for the WireIn to use. These are the Wireable intrfaces ports.
    // The Wireable ports are used to wire up the IEnumerable ports that go in the opposite direction.
    // Note that these are wired in the opposite direction to the dataflow. Tht's becasue this is a pull type monad
    // Indeed the ALA version is based on this class. but will be wired up using the WireIn operator instead.



    class EnumerableMonad<T, U> : IWireForward, IEnumerator<U>, IEnumerable<U>
    {
        //------------------------------------------------------------------------
        // implement the constructor


 
        private readonly Func<T, IEnumerable<U>> function;
        public EnumerableMonad(Func<T, IEnumerable<U>> function) { this.function = function; }


        //------------------------------------------------------------------------
        // Implement the IWireForward interface
        // When using a pull type programming paradigm, in this case iEnumerable, teh WireTo method would have to be used backwards if we wired the IEnumerable ports directly.
        // The WireForward interface allows us to use WireTo or WireIn in the same direction as the dataflow. 
        // see the documentation in the WireForward interface to understand why we use the interface WireForward.

        private IWireForward wireForward;  // This is the port that actually gets wired by WireTo or WireIn.
        private IEnumerable<T> source;  // main input port. This port is not wired directly by WireTo, but indirectly by the use of the WireForward interface.


        // This runs on the source of the dataflow
        private void wireForwardInitialize()  // This is called by WireTo immediately after the wireForward port is wired.
        {
            wireForward.Push(this);
        }


        void IWireForward.Push(object o)
        {
            source = (IEnumerable<T>)o;
        }





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




    class ToEnumerableMonad<T> : IWireForward, IEnumerable<T>
    {
        //------------------------------------------------------------------------
        // implement the constructor



        public ToEnumerableMonad(IEnumerable<T> source) { this.source = source; }


        //------------------------------------------------------------------------
        // Implement the IWireForward interface
        // When using a pull type programming paradigm, in this case iEnumerable, teh WireTo method would have to be used backwards if we wired the IEnumerable ports directly.
        // The WireForward interface allows us to use WireTo or WireIn in the same direction as the dataflow. 
        // see the documentation in the WireForward interface to understand why we use the interface WireForward.

        private IWireForward wireForward;  // This is the port that actually gets wired by WireTo or WireIn.
        private IEnumerable<T> source;  // main input port. This port is not wired directly by WireTo, but indirectly by the use of the WireForward interface.


        // This runs on the source of the dataflow
        private void wireForwardInitialize()  // This is called by WireTo immediately after the wireForward port is wired.
        {
            wireForward.Push(this);
        }


        void IWireForward.Push(object o)
        {
            source = (IEnumerable<T>)o;
        }





        //------------------------------------------------------------------------
        // Implement the IEnumerable interface

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return source.GetEnumerator();
        }

    }

}
