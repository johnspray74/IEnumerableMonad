using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace DomainAbstractions
{
    /// <summary>
    /// ALA domain abstraction for configuring with a LINQ query
    /// This allows you to create wireable ALA domain abstraction objects, but use LINQ to do the actual work.
    /// The domain abstraction has an IObservable input port and an IObservable output port, so is wireable to an IObservable source and to something that needs an IObseravable.
    /// You configure it using a partial LINQ. expression (which is a LINQ expression with a proxy source)
    /// You will first need to create an ObservableProxySource on which to write your LINQ query.
    /// The ObservableProxySource is passed into the constructor along with the query:
    /// example of use
    /// var proxySource = new ObservableProxySource();
    /// source.WireInR(new Query(proxySource, proxySource.Select(x=>x+1).Filer(x=>x%2==0))).WireInR(new ObservableConsoleOutput())

    /// This class is also intended as a pattern for any time you want to create a domain abstraction with different ports than just an input and an output,
    /// and you want to configure what the domain abstraction does using LINQ, 
    /// or you just want to use LINQ internally, you can use the ObservableProxyClass
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    class ObservableQuery<T, U> : IObservable<U>  // output port
    {
        //------------------------------------------------------------------------
        // implement the constructor

        public string instanceName { get; set; } = null;

        private readonly Subject<T> subject;
        private readonly IObservable<U> query;
        public ObservableQuery(Subject<T> subject, IObservable<U> query) { this.subject = subject; this.query = query; }

        private IObservable<T> input;  // input port


        //------------------------------------------------------------------------
        // Implement the IObseravble interface

        private IObserver<U> outputObserver;  // observer in the next domain abstraction


        IDisposable IObservable<U>.Subscribe(IObserver<U> observer)
        {
            if (input == null) throw new NullReferenceException();
            outputObserver = observer;
            // When we get subscribed to, do the subscriptions to both the query and our input
            query.Subscribe(
                (x) => outputObserver.OnNext(x),        // output from query goes to the output
                (ex) => outputObserver.OnError(ex),     // any new exception from the query goes to the output
                () => { }   // OnComplete from the query is discarded, OnComplete comes from the input
                );
            return input.Subscribe(
                (x) => subject.OnNext(x), // data from the input goes to the query - the subject is the input to the query 
                (ex) => outputObserver.OnError(ex),    // exceptions from input are passed through to the output
                () => outputObserver.OnCompleted()     // OnComplete from the input is passed through to the output
                );
        }
    }

}
