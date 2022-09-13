using ProgrammingParadigms;
using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace DomainAbstractions
{
    /// <summary>
    /// ALA domain abstraction for configuring with a LINQ query
    /// This example shows how to create an ALA domain abstraction using any programming paradigm and be able to use LINQ to do the actual work.
    /// In this case the programming paradigm we will use is IObservablePush, but this could be any dataflow type programming paradigm.
    /// You configure it with a LINQ expression.
    /// When writing the LINQ expression, you need an IObservable as a starting point. Just use a Subject object for this.
    /// Then configure the instance of this domain abstraction with both the subject and the query itself:
    /// example of use
    /// var source = new Subject<T>();
    /// var query = source.Select(x=>x+1).Filer(x=>x%2==0);
    /// new ObservableQuery<T, U>(source, query);
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    class ObservableQuery<T, U> : IObserverPush<T>  // input port
    {
        public string instanceName { get; set; } = null;

        //------------------------------------------------------------------------
        // implement the constructor

        private readonly Subject<T> queryFrontEnd;
        private readonly IObservable<U> query;
        public ObservableQuery(Subject<T> queryFrontEnd, IObservable<U> query) { this.queryFrontEnd = queryFrontEnd; this.query = query; }

        private IObserverPush<U> output;  // output port


        //------------------------------------------------------------------------
        // Implement the IObseravble interface


        void IObserverPush<T>.OnStart()
        {
            output.OnStart();
            query.Subscribe(
                (data) => output.OnNext(data),        // route output from query to the output of the domain abstraction
                (ex) => output.OnError(ex),           // route exceptions from the query to the output
                () => output.OnCompleted()            // route OnCompleted from the query to the output
                );
        }

        void IObserverPush<T>.OnNext(T data)
        {
            queryFrontEnd.OnNext(data);
        }

        void IObserverPush<T>.OnCompleted()
        {
            output.OnCompleted();
        }

        void IObserverPush<T>.OnError(Exception ex)
        {
            output.OnError(ex);
        }
    }

}
