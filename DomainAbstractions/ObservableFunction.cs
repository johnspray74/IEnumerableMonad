// #define UseCreate



using Foundation;
using System;
using System.Reactive.Disposables;

namespace DomainAbstractions
{

    // extension methods for the ObservableMonadF

    static partial class ExtensionMethods
    {


        public static IObservable<U> Bind<T, U>(this IObservable<T> source, Func<T, IObservable<U>> function)
        {
            return (IObservable<U>) source.WireInR(new ObservableFunction<T, U>(function));
        }


    }





    // This is the domain abstraction class
    // It has one input port and one output port of type IObservable.
    // Instances of this class can be wired to each other using these ports with WireInR.
    // WireInR is to logically wire A to B, logically meaning that the dataflow is in the direction A to B, however the actual wiring using the Ioberver interface is from B to A.
    // B must then use the IObservable to subscribe itself to A. 
    // A good time to do this is when B is itself subsccribed to.
    // Instances of this class can be wired to any IObservable source.
    // It is used like this:
    // .WireInR(new IObservableMonadF(lambda)), where lambda is a function that takes a T and returns an IObservable<U>. 
    // The lambdas returned IObservable can of course push out multiple values.

    // For demonstration purposes it can also be used like this so that the syntax is identical to normal monads:
    // .Bind(lambda)
    // However the ALA version would normally bother making a Bind function for this one type of wiring, when many other wirings are without a Bind function.
    // Bind will pass it the source IObservable, and the application function in the constructor.

    // Note that this monad is a deferred/push type of monad. 
    // The class is basically driven by the OnNext method, which is called by the previous monad in the chain.
    // OnNext does everything.
    // The class is completely lazy, so it doesn't even subscribe to its source until it is itself subscribed to.

    // Although this class has nothing to do with ALA (it is solely to support the monad version), note how similar it is to an ALA domain abstraction class.
    // It has an input "port" which is the source field, which is wired by the Bind function via the constructor.
    // It has an output "port", which is the implemented IEnumerable interface, which is wired by the next Bind function.
    // Note that these are wired in the opposite direction to the dataflow. Tht's becasue this is a pull type monad
    // Indeed the ALA version is based on this class. but will be wired up using the WireIn operator instead.
    class ObservableFunction<T, U> : IObserver<T>, IObservable<U>  // IObservable is the output port, IObserver is used to subscribe to the previous instance
    {
        //------------------------------------------------------------------------
        // implement the constructor

        private readonly Func<T, IObservable<U>> function;
        public ObservableFunction(Func<T, IObservable<U>> function) { this.function = function; }


        private IObservable<T> input;   // This is wired by WireInR to the previous monad object
        private IObserver<U> nextObserver;    // This is wired by the Subscribe method to the next monad object


        //------------------------------------------------------------------------
        // implement the IObservable
        // This interface is called by the next monad down the chain to give us its observer

        IDisposable IObservable<U>.Subscribe(IObserver<U> observer)
        {
            nextObserver = observer;
            input.Subscribe(this);
            return Disposable.Empty;
        }


        //------------------------------------------------------------------------
        // implement the IObserver
        void IObserver<T>.OnCompleted()
         {
            nextObserver.OnCompleted();
        }

        void IObserver<T>.OnError(Exception ex)
        {
            nextObserver.OnError(ex);
        }

        void IObserver<T>.OnNext(T value)
        {
            // Each time we get a value, call this function which returns an IObservable, then subscribe the nextObsever to that IObservable 
            function(value).Subscribe(
                (x) => nextObserver.OnNext(x),
                (ex) => nextObserver.OnError(ex),
                () => { Console.Write("#"); }   // intercep OnComplete, we are combining the IObservables from the function
                );
        }
    }
}
