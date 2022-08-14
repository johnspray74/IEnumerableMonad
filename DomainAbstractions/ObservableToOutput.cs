using Foundation;
using ProgrammingParadigms;
using System;
using System.Dynamic;
using System.Reactive.Disposables;

namespace DomainAbstractions
{
    // static version
    class ObservableToOutput<T> : IEvent, IObservable<T> where T : class
    {
        private string separator;
        private string Separator { set { separator = value; } }

        private IObservable<T> source;   // input port

        public event OutputDelegate output;   // output port


        void IEvent.Execute()
        {
            source.Subscribe(
                             (x) => output?.Invoke($"{x.ClassToString()}{Environment.NewLine}"),
                             (ex) => output?.Invoke($"Exception {ex}"),
                              () => output?.Invoke($"Complete{Environment.NewLine}")
                    );
        }


        // used for Sniff only
        IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
        {
            source.Subscribe(
                             (x) => { output?.Invoke($"{x.ClassToString()}{Environment.NewLine}"); observer.OnNext(x); },
                             (ex) => { output?.Invoke($"Exception {ex}"); observer.OnError(ex);  } ,
                              () => { output?.Invoke($"Complete{Environment.NewLine}"); observer.OnCompleted(); }
                    );
            return Disposable.Empty;
        }
    }




    public delegate void OutputDelegate(string output);


    static partial class ExtensionMethods
    {
        public static ObservableToOutput<T> ToOutput<T>(this IObservable<T> observable) where T : class { var o = new ObservableToOutput<T>(); observable.WireInR(o); return o; }
        public static IObservable<T> Sniff<T>(this IObservable<T> observable) where T : class { var oto = new ObservableToOutput<T>(); oto.output += Console.Write;  observable.WireInR(oto); return oto; }
    }







    // dynamic version
    // input port is IObservable<object> whereas the static version the input port is IObservable<T> 
    // Actually the static version will also handle dynamic because it can take a type object and will handle an ExpandoClass
    // The only advantage of this version is you can use WireTo without providing a type on the new, whereas to use the static version in a dynamic you would need to provide the type (either object or ExpandoClass),
    // or use the ToOutput extension method to make type inference work.
    class ObservableToOutput : IEvent
    {
        private string separator;
        private string Separator { set { separator = value; } }

        private IObservable<object> source;   // input port

        public delegate void OutputDelegate(string output);
        public event OutputDelegate output;   // output port


        void IEvent.Execute()
        {
            source.Subscribe(
                             (x) => output?.Invoke($"{x.ClassToString()}{Environment.NewLine}"),
                             (ex) => output?.Invoke($"Exception {ex}"),
                              () => output?.Invoke($"Complete{Environment.NewLine}")
                    );
        }
    }
}
