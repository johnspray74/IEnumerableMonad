
using System;
using System.Reactive;


// This interface is like IObserver but is intentionally different so it cant be used to wire with IObservable.
// It has no associate IObservable becasue it does not initiate data transfer using Subscribe.
// Instead, the source initiates data transfer so it is a true push programming paradigm (can work truly asynchronously and will work over networks)
// The OnStart method may not be strictly necessary. but makes the programming paradigm easier to understand
// Note that in the IObservable/IObserver pair, wiring is done on the IObservable, (in the reverse direction of dataflow)
// and then its Subscribe method is used to initiate each data transfer sequence.
// The IObserverPush interface is wired directly in the direction of dataflow. It can handle multiple sequences of data using OnStart().
// The reason why this interface cannot use IObserver is that IObserver implemenations generally stop working when OnCompleted or OnError is called.
// You have to re-call the Subscribe method in the associated IObserveble to get them working again.
// In the IObserverInterface, OnStart will get it working again afer OnCompleted or OnError is called.

namespace ProgrammingParadigms
{
    interface IObserverPush<T> : IObserver<T>
    {
        void OnStart();
        /*
        The IObserver adds the following methods:
        void IObserver<T>.OnNext(T data);
        void IObserver<T>.OnCompleted();
        void IObserver<T>.OnError(Exception ex);
        */
    }
}
