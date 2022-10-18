using Foundation;
using ProgrammingParadigms;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Reactive.Disposables;

namespace DomainAbstractions
{

    // This is an ALA domain abstraction with an input IObservable<T> port that converts to text and outputs to a delegate that takes a string parameter
    // static version - uses a generic type T (see below for dynamic version)
    // ObservableToSerial is at the end of an IObservable chain, so it intiates the data transfer - it has an IEvent input for this purpose.




    class ObservableToSerial<T> : IEvent  // start port
    {
        public ObservableToSerial(ObservableToSerialDelegate output) { this.output = output; }

        private IObservable<T> source;   // input port

        public event ObservableToSerialDelegate output;   // output delegate for text


        void IEvent.Execute()
        {
            source.Subscribe(
                             (x) => output?.Invoke($"{x.ObjectToString()} "),
                             (ex) => output?.Invoke($"Exception {ex}"),
                             () => output?.Invoke($"Complete{Environment.NewLine}"));
        }
    }






    delegate void ObservableToSerialDelegate(string output);


    static partial class ExtensionMethods
    {
        public static ObservableToSerial<T> ToSerial<T>(this IObservable<T> observable, ObservableToSerialDelegate output) where T : class { var o = new ObservableToSerial<T>(output); observable.WireInR(o); return o; }
    }



    // dynamic version
    // input port is IObservable<object> whereas the static version the input port is IObservable<T> 
    // Actually the static version will also handle dynamic because it can take a type object and will handle an ExpandoClass
    // The only advantage of this version is you can use WireTo without providing a type on the new, whereas to use the static version in a dynamic you would need to provide the type (either object or ExpandoClass),
    // or use the ToSerial extension method to make type inference work.
    class ObservableToSerial : IEvent
    {
        private IObservable<object> source;   // input port

        public event ObservableToSerialDelegate output;   // output port

        public ObservableToSerial(ObservableToSerialDelegate output) { this.output = output; }

        private IDisposable subscription = null;

        void IEvent.Execute()
        {
            subscription?.Dispose();
            subscription = source.Subscribe(
                             (x) => output?.Invoke($"{x.ObjectToString()}{Environment.NewLine}"),
                             (ex) => output?.Invoke($"Exception {ex}"),
                             () => output?.Invoke($"Complete{Environment.NewLine}")
                    );
        }
    }




    static partial class ExtensionMethods
    {
        // if monads, and prefer monad syntax instead of WireInR, this allows output at the end using e.g. .ToSerial(Console.WriteLine).
        public static ObservableToSerial ToSerial(this IObservable<object> observable, ObservableToSerialDelegate output) { var o = new ObservableToSerial(output); observable.WireInR(o); return o; }
    }

}
