// #define UseCreate

// Monad.ObservableMonadF namespace, Bind takes a Func
// Monad.ObservableMonadA namespace, Bind takes an Action // This is simpler - use this one


using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;





namespace Monad.ObservableMonadA
{

    // extension methods for the ObservableMonad

    static partial class ExtensionMethods
    {

#if UseCreate

        public static IObservable<U> Bind<T, U>(this IObservable<T> source, Action<T, IObserver<U>> action)
        {
            // Observable.Create makes an IObservable, you have to give it a subscribe function
            // In this subscribe function we need to subscribe to the source which means providing an OnNext, Oncompleted, and OnError for the source to call.
            // The Oncompleted and OnError need to pass straight through to the outputObserver
            // The OnNext needs to call the action and pass it an observer.
            // We can't just pass it the outputObserver directly, becasue the OnCompleted call coming out of the Action needs to be intercepted.
            // (When the Action completes, it doesn't mean the entoire output sequence has completed
            // So we have to give action an loaclly created observer
            return Observable.Create<U>(outputObserver =>
            {
                source.Subscribe(x => 
                    {
                        // each time we get a value from the source, call the action and give the output observer to it.
                        action(x, Observer.Create<U>(
                            value => outputObserver.OnNext(value),
                            ex => outputObserver.OnError(ex),
                            () => { }
                        ));
                    },
                    ex => outputObserver.OnError(ex),
                    () => outputObserver.OnCompleted()
                );
                return Disposable.Empty;
            });
        }




#else


        public static IObservable<U> Bind<T, U>(this IObservable<T> source, Action<T, IObserver<U>> action)
        {
            return new Observable<T, U>(source, action);
        }




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
            private readonly Action<T, IObserver<U>> action;
            public Observable(IObservable<T> source, Action<T, IObserver<U>> action) { this.source = source; this.action = action; }



            //------------------------------------------------------------------------
            // implement the IObservable
            // This interface is called by the next monad down the chain to give us its observer

            private IObserver<U> output;
            private ObserverDecorator<U> innerObserver;
            private IDisposable subscription = null;

            IDisposable IObservable<U>.Subscribe(IObserver<U> observer)
            {
                output = observer;
                // when the source produces numbers, they will go to the IObserver interface of this object, so will apear at the OnNext method below..
                innerObserver = new ObserverDecorator<U>(
                    value => output.OnNext(value),
                    ex => output.OnError(ex),
                    () => { }
                    );
                subscription?.Dispose();
                subscription = source.Subscribe(this);
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
            }

            void IObserver<T>.OnNext(T value)
            {
                action(value, innerObserver);  // intercep OnComplete, we are combining the IObservables from the function
            }


            // Simple IObserver class where you provide the OnNext, OnClomplete and OnError methods in the constructor.
            // Note that Observer.Create could be used instead of this class, however Observer.Create would need to be called for every call of the action function
            // otherwise it would stop working if it gets an OnCompleted or OnError call from the action.
            // By using ObserverDecorator which just passes through everything, we can instantiate it once.
            private class ObserverDecorator<T> : IObserver<T>
            {
                //------------------------------------------------------------------------
                // implement the constructor

                private Action<T> onNext;
                private Action onCompleted;
                private Action<Exception> onError;

                public ObserverDecorator(Action<T> onNext, Action<Exception> onError, Action onCompleted) { this.onNext = onNext; this.onCompleted = onCompleted; this.onError = onError; }

                //------------------------------------------------------------------------
                // implement the IObserver
                void IObserver<T>.OnCompleted() => onCompleted();

                void IObserver<T>.OnError(Exception ex) => onError(ex);

                void IObserver<T>.OnNext(T value) => onNext(value);
            }

        }

#endif
    }
}
