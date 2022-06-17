
using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;


namespace DomainAbstractions
{
    class ValueToObservable<T> : IObservable<T>
    {
        private T value;

        public ValueToObservable(T value) { this.value = value; }


        IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
        {
            return Observable.Create<T>(observer =>
           {
               observer.OnNext(value); 
               observer.OnCompleted();
               return Disposable.Empty;
           }).Subscribe(observer);
        }

    }
}
