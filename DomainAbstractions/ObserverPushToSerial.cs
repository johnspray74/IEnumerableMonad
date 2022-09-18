using Foundation;
using ProgrammingParadigms;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Reactive.Disposables;

namespace DomainAbstractions
{

    // This is an ALA domain abstraction with an input IObservable<T> port that converts to text and outputs to a delegate that takes a string parameter
    // static version - uses a generic type T (However T can be ExpendoObject)
    // ObservableToSerial is at the end of an IObservable chain, so it intiates the data transfer - it has an IEvent input for this purpose.




    class ObserverPushToSerial<T> : IObserverPush<T>  // input port
    {
        public ObserverPushToSerial(ObserverPushSerialDelegate output) { this.output = output; }

        private IObservable<T> source;   // input port

        public event ObserverPushSerialDelegate output;   // output delegate for text


        void IObserverPush<T>.OnStart()
        {
        }

        void IObserver<T>.OnNext(T data)
        {
            output?.Invoke($"{data.ObjectToString()} ");
        }

        void IObserver<T>.OnCompleted()
        {
            output?.Invoke($"Complete{Environment.NewLine}");
        }

        void IObserver<T>.OnError(Exception ex)
        {
            output?.Invoke($"Exception {ex}{Environment.NewLine}");
        }
    }






    delegate void ObserverPushSerialDelegate(string output);


    static partial class ExtensionMethods
    {
        public static ObserverPushToSerial<T> ToSerial<T>(this IObserverPush<T> observerPush, ObserverPushSerialDelegate output) where T : class { var o = new ObserverPushToSerial<T>(output); observerPush.WireInR(o); return o; }
    }



    // dynamic version
    // input port is IObservable<object> whereas the static version the input port is IObservable<T> 
    // Actually the static version will also handle dynamic because it can take a type object and will handle an ExpandoObject class
    // The advantage of this version is you can use WireTo without providing a type on the new, whereas to use the static version in a dynamic you would need to provide the type (either object or ExpandoObject),
    // or use the ToSerial extension method to make type inference work.
    class ObserverPushToSerial : IObserverPush<object>
    {
        public event ObservableToSerialDelegate output;   // output port

        public ObserverPushToSerial(ObservableToSerialDelegate output) { this.output = output; }

        void IObserverPush<object>.OnStart()
        {
            output?.Invoke($"Start ");
        }

        void IObserver<object>.OnNext(object data)
        {
            output?.Invoke($"{data.ObjectToString()} ");
        }

        void IObserver<object>.OnCompleted()
        {
            output?.Invoke($"Complete{Environment.NewLine}");
        }

        void IObserver<object>.OnError(Exception ex)
        {
            output?.Invoke($"Exception {ex}");
        }
    }


}
