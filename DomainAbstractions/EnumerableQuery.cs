using ProgrammingParadigms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Markup;

namespace DomainAbstractions
{



    /// <summary>
    /// ALA domain abstraction for configuring with a LINQ query
    /// The domain abstractions has an iEnumerable input port and an IEnumerable output port, so is wireable to an iEnumerable source and to something that needs an IEnumerable source.
    /// e.g.
    /// source.WireInR(new EnumerableQuery(configure with a LINQ query)).WireInR(new IEnumerableConsoleOutput());
    /// You configure it using LINQ
    /// This allows you to create wireable ALA domain abstraction objects, but use LINQ to do the actual work.
    /// This class is also intended as a pattern for any time you want to create a domain abstraction with different ports than just an input and an output, but you want to configure it with LINQ
    /// You configure this class by passing in a LINQ query.
    /// You will first need to create an EnumerableProxySource on which to write your LINQ query.
    /// The EnumrableProxySource is passed into the constructor as well as the query:
    /// 
    /// var proxySource = new EnumerableProxySource();
    /// source.WireInR(new Query(proxySource, proxySource.Select(x=>x+1).Filer(x=>x%2==0))).WireInR(new EnumerableConsoleOutput());
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    class EnumerableQuery<T, U> : IDataFlow<T>  // input port
    {
        //------------------------------------------------------------------------
        // implement the constructor

        public string instanceName { get; set; } = null;

        private readonly EnumerableProxySource<T> proxySource;
        private readonly IEnumerable<U> query;
        public EnumerableQuery(EnumerableProxySource<T> proxySource, IEnumerable<U> query) { this.proxySource = proxySource; this.query = query; proxySource.Enumerable = Values(); }

        private IDataFlow<U> output;  // output port

        private T value;

        private IEnumerable<T> Values()
        {
            yield return value;
        }

        //------------------------------------------------------------------------
        // Implement the IDataFlow interface

        void IDataFlow<T>.Push(T value)
        {
            this.value = value;
            foreach (var x in query) output?.Push(x);
        }
    }



    /// <summary>
    /// Allows you to build a query when you don't have the source. 
    /// This is a proxy IEnumerable
    /// It implementes IEnumerable and you give it an IEnumerable via a setter later.
    /// When the Implemented IEnumerable is used, it simple passes through to the stored IEnumerable.
    /// It gives you a starting object to build the middle of a query, and then connect that query up to a source later via a setter.
    /// Example:
    /// var proxySource = new ProxySource<int>(); var query = queryStart.Select(x=>x+1).Filter(x=>x%2==0);
    /// Somewhere else in your code later you can use the query after connecting it to an actual source:
    /// proxySource.Enumerable = actualSource; // var result = query.ToList();
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    class EnumerableProxySource<T> : IEnumerable<T>
    {
        public string instanceName { get; set; } = null;

        private IEnumerable<T> enumerable = null;
        public IEnumerable<T> Enumerable { set { enumerable = value; } }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return enumerable.GetEnumerator();
        }
    }

}
