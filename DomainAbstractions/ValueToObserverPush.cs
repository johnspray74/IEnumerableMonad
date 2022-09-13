using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ProgrammingParadigms;

namespace DomainAbstractions
{
    class ValueToObserverPush<T> :IEvent
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
