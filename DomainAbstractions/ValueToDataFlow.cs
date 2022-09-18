using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ProgrammingParadigms;

namespace DomainAbstractions
{
    class ValueToDataFlow<T> : IEvent
    {
        private T value;

        public ValueToDataFlow(T value) { this.value = value; }


        private IDataFlow<T> output;

        void IEvent.Execute()
        {
            output.Push(value);
        }
    }
}
