// #define UseCreate



using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;





namespace Monad.ObservableMonad2
{

    // extension methods for the ObservableMonad

    public static class MonadExtensionMethods
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
            return Observable.Create<U>(outputObserver=>
            {
                source.Subscribe(x => 
                {
                // each time we get a value from the source, call the action and give the output observer to it.
                action(x, new ObserverDecorator<U>(
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



        // Simple IObserver class where you provide the OnNext, OnClomplete and OnError methods in the constructor.
        private class ObserverDecorator<U> : IObserver<U>
        {
            //------------------------------------------------------------------------
            // implement the constructor

            private Action<U> onNext;
            private Action onCompleted;
            private Action<Exception> onError;

            public ObserverDecorator(Action<U> onNext, Action<Exception> onError, Action onCompleted) { this.onNext = onNext; this.onCompleted = onCompleted; this.onError = onError; }

            //------------------------------------------------------------------------
            // implement the IObserver
            void IObserver<U>.OnCompleted() => onCompleted();

            void IObserver<U>.OnError(Exception ex) => onError(ex);

            void IObserver<U>.OnNext(U value) => onNext(value);
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
            }

            void IObserver<T>.OnNext(T value)
            {
                action(value, new ObserverDecorator<U>(
                    value => output.OnNext(value),
                    ex => output.OnError(ex),
                    () => { }
                    )  // intercep OnComplete, we are combining the IObservables from the function
                );
            }
        }
#endif


        // Simple IObserver class where you provide the OnNext, OnClomplete and OnError methods in the constructor.
        private class ObserverDecorator<U> : IObserver<U>
        {
            //------------------------------------------------------------------------
            // implement the constructor

            private Action<U> onNext;
            private Action onCompleted;
            private Action<Exception> onError;

            public ObserverDecorator(Action<U> onNext, Action<Exception> onError, Action onCompleted) { this.onNext = onNext; this.onCompleted = onCompleted; this.onError = onError; }

            //------------------------------------------------------------------------
            // implement the IObserver
            void IObserver<U>.OnCompleted() => onCompleted();

            void IObserver<U>.OnError(Exception ex) => onError(ex);

            void IObserver<U>.OnNext(U value) => onNext(value);
        }


    }
}
