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
    // ObservableToOutput is at the end of an Iobservable chain, so it intiates the data transfer - it has an iEvent input for this purpose.


    class ObservableToOutput<T> : IEvent  // start port
    {
        public ObservableToOutput(OutputDelegate output) { this.output = output; }

        private IObservable<T> source;   // input port

        public event OutputDelegate output;   // output delegate for text


        void IEvent.Execute()
        {
            source.Subscribe(
                             (x) => output?.Invoke($"{x.ObjectToString()} "),
                             (ex) => output?.Invoke($"Exception {ex}"),
                              () => output?.Invoke($"Complete{Environment.NewLine}"));
        }
    }






    delegate void OutputDelegate(string output);


    static partial class ExtensionMethods
    {
        public static ObservableToOutput<T> ToOutput<T>(this IObservable<T> observable, OutputDelegate output) where T : class { var o = new ObservableToOutput<T>(output); observable.WireInR(o); return o; }
    }



    // dynamic version
    // input port is IObservable<object> whereas the static version the input port is IObservable<T> 
    // Actually the static version will also handle dynamic because it can take a type object and will handle an ExpandoClass
    // The only advantage of this version is you can use WireTo without providing a type on the new, whereas to use the static version in a dynamic you would need to provide the type (either object or ExpandoClass),
    // or use the ToOutput extension method to make type inference work.
    class ObservableToOutput : IEvent
    {
        private IObservable<object> source;   // input port

        public event OutputDelegate output;   // output port

        public ObservableToOutput(OutputDelegate output) { this.output = output; }

        void IEvent.Execute()
        {
            source.Subscribe(
                             (x) => output?.Invoke($"{x.ObjectToString()}{Environment.NewLine}"),
                             (ex) => output?.Invoke($"Exception {ex}"),
                              () => output?.Invoke($"Complete{Environment.NewLine}")
                    );
        }
    }




    static partial class ExtensionMethods
    {
        // if monads, and prefer monad syntax instead of WireInR, this allows output at the end using e.g. .ToOutput(Console.WriteLine).
        public static ObservableToOutput ToOutput(this IObservable<object> observable, OutputDelegate output) { var o = new ObservableToOutput(output); observable.WireInR(o); return o; }
    }

}
