using Foundation;
using System;
using System.Diagnostics;
using System.Reactive.Disposables;

namespace DomainAbstractions
{
    // IObserable<T> Decorator used by Sniff function
    // T can be normal object or ExpandoObject
    class SniffDecorator<T> : IObservable<T>  // output
    {
        public SniffDecorator(OutputDelegate output) { this.output = output; }

        private IObservable<T> source;   // input port

        public event OutputDelegate output;   // output port


        // pass everything straight through from input to output, but output it to the delegate in text form as well.
        IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
        {
            source.Subscribe(
                             (x) => { output?.Invoke($"{x.ObjectToString()}{Environment.NewLine}"); observer.OnNext(x); },
                             (ex) => { output?.Invoke($"Exception {ex}"); observer.OnError(ex); },
                              () => { output?.Invoke($"Complete{Environment.NewLine}"); observer.OnCompleted(); }
                    );
            return Disposable.Empty;
        }
    }




    // Allows inserting .Sniff() into a chain of monads, which will output to debug window the type of the dataflow at that point and the data flowing past that point.
    static partial class ExtensionMethods
    {
        // T can be any class including ExpandoObject
        public static IObservable<T> Sniff<T>(this IObservable<T> observable) where T : class { var oto = new SniffDecorator<T>((x) => Debug.Write(x)); observable.WireInR(oto); return oto; }
    }
}
