// #define UseCreate



using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Foundation;
using ProgrammingParadigms;

namespace DomainAbstractions
{

    // extension methods for the ObservableMonad

    public static class MonadExtensionMethods
    {

#if UseCreate

        public static IObservable<U> Bind<T, U>(this IObservable<T> source, Func<T, IObservable<U>> function)
        {
            return Observable.Create<U>(outputObserver =>
            {
                source.Subscribe(x =>
                {
                    // each time we get a value from the source, call the funtion which will return another IObservable. Then subsribe the output observer to it.
                    function(x).Subscribe(x => outputObserver.OnNext(x));
                },
                ex => outputObserver.OnError(ex),
                () => outputObserver.OnCompleted()
                );
                return Disposable.Empty;
            });
        }


#else

        public static IObservable<U> Bind<T, U>(this IObservable<T> source, Func<T, IObservable<U>> function)
        {
            return (IObservable<U>) source.WireIn(new ObserverMonad<T, U>(function));
        }

#endif

        public static IObservable<T> ToWireableObserver<T>(this IObservable<T> source)
        {
            return new WireableObserver<T>(source);
        }


        public static T Cast<T>(this object o) where T : class { return (T)o; }


    }

    // This is the class that will be used by Bind to implement the monad.
    // Bind will pass it the source IObservable, and the application function in the constructor.
    // Note that this monad is a deferred/push type of monad. 
    // The class is basically driven by the OnNext method, which is called by the previous monad in the chain.
    // OnNext does everything.
    // The class is completely lazy, so it doesn't even get the source IEnumerator from the source IEnumerable until the first call of MoveNext.

    // Although this class has nothing to do with ALA (it is solely to support the monad version), note how similar it is to an ALA domain abstraction class.
    // It has an input "port" which is the source field, which is wired by the Bind function via the constructor.
    // It has an output "port", which is the implemented IEnumerable interface, which is wired by the next Bind function.
    // Note that these are wired in the opposite direction to the dataflow. Tht's becasue this is a pull type monad
    // Indeed the ALA version is based on this class. but will be wired up using the WireIn operator instead.



    class ObserverMonad<T, U> : IObserver<T>, IObservable<U>
    {
        //------------------------------------------------------------------------
        // implement the constructor


        private readonly Func<T, IObservable<U>> function;
        public ObserverMonad(Func<T, IObservable<U>> function) { this.function = function; }


        /*
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
        */
        // Instead of using IWireForward and the IObservable for subscribing, we can just let WireIn wire up the output directly


        private IObserver<U> output;


        //------------------------------------------------------------------------
        // implement the IObservable
        // This interface is called by the next monad down the chain to give us its observer


        IDisposable IObservable<U>.Subscribe(IObserver<U> observer)
        {
            output = observer;
            return Disposable.Empty;
        }




        //------------------------------------------------------------------------
        // implement the IObserver
        void IObserver<T>.OnCompleted()
        {
            output.OnCompleted();
        }

        void IObserver<T>.OnError(Exception ex)
        {
            output.OnError(ex);
            throw new NotImplementedException();
        }

        void IObserver<T>.OnNext(T value)
        {
            // Each time we get 
            function(value).Subscribe(output);
        }
    }





    // This class goes from an iObservable to a class that can be Wired with WireIn.
    class WireableObserver<T> : IObservable<T>
    {
        private readonly IObservable<T> source;
        public WireableObserver(IObservable<T> source) { this.source = source; }



        private IObserver<T> output;   // ALA output port. When it is wired, outputInitialize is called, causing the source to be subscribed to

        private void outputInitialize()
        {
            source.Subscribe(output);
        }


        // The IObservable interface is not used, but is needed for Bind to be used on this class.
        IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
        {
            throw new NotImplementedException();
        }
    }


}
