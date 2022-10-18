using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ProgrammingParadigms;

namespace DomainAbstractions
{
    // This is a simple domain abstraction that has an input iEvent port and an output IObserverPush port
    // It is configured with a value.
    // When an event arives at the input, it outputs the value, complete with OnStart and OnCompleted calls.




    class ValueToObserverPush<T> : IEvent, IBindable<T>
    {
        private T value;

        public ValueToObserverPush(T value) { this.value = value; }


        private IObserverPush<T> output;

        void IEvent.Execute()
        {
            output.OnStart();
            output.OnNext(value);
            output.OnCompleted();
        }
    }

        
    static partial class ExtensionMethods
    {
        static public ValueToObserverPush<T> ToObserverPush<T>(this T value) { return new ValueToObserverPush<T>(value); }
    }
}
