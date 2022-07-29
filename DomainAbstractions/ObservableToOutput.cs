using Foundation;
using ProgrammingParadigms;
using System;

namespace DomainAbstractions
{
    class ObservableToOutput<T> : IEvent where T : class
    {
        private string separator;
        private string Separator { set { separator = value; } }

        private IObservable<T> source;   // input port
                                         
        public delegate void OutputDelegate(string output);
        public event OutputDelegate output;   // output port


        void IEvent.Execute()
        {
            source.Subscribe(
                             (x) => output?.Invoke($"{x.ClassToString()} "),
                             (ex) => output?.Invoke($"Exception {ex}"),
                              () => output?.Invoke("Complete")
                    );
        }
    }






    static class ObservableToConsoleOutputExtensionMethod
    {
        public static ObservableToOutput<T> ToOutput<T>(this IObservable<T> observable) where T : class { var o = new ObservableToOutput<T>(); observable.WireInR(o); return o; }
    }
}
