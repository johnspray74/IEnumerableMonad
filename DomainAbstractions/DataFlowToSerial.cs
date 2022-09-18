using Foundation;
using ProgrammingParadigms;
using System;

namespace DomainAbstractions
{
    // This is an ALA domain abstraction with an input IDataFlow<T> port that converts to text and outputs to a delegate that takes a string parameter
    // static version - uses a generic type T (However T can be ExpandoObject)
    // ObservableToSerial is at the end of an IObservable chain, so it intiates the data transfer - it has an IEvent input for this purpose.


    class DataFlowToSerial<T> : IDataFlow<T>  // start port
    {
        public DataFlowToSerial(DataFlowToSerialDelegate output) { this.output = output; }

        public event DataFlowToSerialDelegate output;   // output delegate for text


        void IDataFlow<T>.Push(T data)
        {
            output?.Invoke($"{data.ObjectToString()} ");
        }

    }



    delegate void DataFlowToSerialDelegate(string output);



    static partial class ExtensionMethods
    {
        public static DataFlowToSerial<T> ToSerial<T>(this IDataFlow<T> dataflow, DataFlowToSerialDelegate output) where T : class { var o = new DataFlowToSerial<T>(output); dataflow.WireInR(o); return o; }
    }




    // dynamic version
    // input port is IDataFlow<object> whereas the static version the input port is IDataFlow<T> 
    // Actually the static version will also handle dynamic because it will handle an ExpandoObject class
    // The advantage of this version is you can use WireTo without providing a type on the new, whereas to use the static version in a dynamic you would need to provide the type (either object or ExpandoObject),
    // or use the ToSerial extension method to make type inference work.
    class DataFlowToSerial : IDataFlow<object>
    {
        public event DataFlowToSerialDelegate output;   // output port

        public DataFlowToSerial(DataFlowToSerialDelegate output) { this.output = output; }


        void IDataFlow<object>.Push(object data)
        {
            output?.Invoke($"{data.ObjectToString()} ");
        }
    }

}