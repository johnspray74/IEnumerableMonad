// #define UseCreate

// Monad.ObservableMonadF namespace, Bind takes a Func
// Monad.ObservableMonadA namespace, Bind takes an Action // This is simpler - use that one


using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;





namespace Monad.ObservableMonadF
{

    // extension methods for the ObservableMonad

    static partial class ExtensionMethods
    {

#if UseCreate

        public static IObservable<U> Bind<T, U>(this IObservable<T> source, Func<T, IObservable<U>> function)
        {
            return Observable.Create<U>(outputObserver=>
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
            return new Observable<T, U>(source, function);
        }



#endif


        // This is the class that will be used by Bind to implement the monad.
        // Bind will pass it the source IObservable, and the application function in the constructor.
        // Note that this monad is a deferred/push type of monad. 
        // The class is basically driven by the OnNext method, which is called by the previous monad in the chain.
        // OnNext does everything.

        // Although this class has nothing to do with ALA (it is solely to support the monad version), note how similar it is to an ALA domain abstraction class.
        // It has an input "port" which is the source field, which is wired by the Bind function via the constructor.
        // It has an output "port", which is the implemented IObseravble interface, which is wired by the next Bind function.
        // Note that these are wired in the opposite direction to the dataflow, despite the fact that its a Push type monad. That's because the IObservable interface is goes in the opposite direction to the dataflow.
        // Indeed the ALA version is a copied modifiction of this class. but will be wired up using the WireIn operator instead.



        private class Observable<T, U> : IObserver<T>, IObservable<U>
        {
            //------------------------------------------------------------------------
            // implement the constructor


            private readonly IObservable<T> source;
            private readonly Func<T, IObservable<U>> function;
            public Observable(IObservable<T> source, Func<T, IObservable<U>> function) { this.source = source; this.function = function; }






            //------------------------------------------------------------------------
            // implement the IObservable
            // This interface is called by the next monad down the chain to give us its observer

            IObserver<U> output;

            IDisposable IObservable<U>.Subscribe(IObserver<U> observer)
            {
                output = observer;
                // when the source produces numbers, they will go to the IObserver interface of this object, so will apear at the OnNext method below..
                source.Subscribe(this);   
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
                function(value).Subscribe(
                    (x) => output.OnNext(x),
                    (ex) => output.OnError(ex),
                    () => { }   // intercep OnComplete, we are combining the IObservables from the function
                );
            }
        }

    }
}
