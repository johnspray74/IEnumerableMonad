﻿// This code is wrtten as example code for chapter 3 section on moands in the online book at abstractionlayeredarchitecture.com
// See that website for full discussion of the comparison between ALA and monads

// In brief ALA and monads are both composition patterns.
// ALA composes objects and monads compose functions.
// ALA is a more gneral composng solution. It is also easier to understand because it uses plain objects.
// Monads, at lead defrred type monads gnerate a lot of code under the covers, which makes them difficult to understand an explain.
// So if ALA is a more general solution, what we can do implement monad behaviour (composition of functions) using ALA.
// This is easily done by creating a programming paradigm to do what the monad interface does,
// and writing a domain abstraction with one input port and one output port of the programming paradigm.
// The domain abstraction takes a configuration parameter via its constructor which is exactly the form of the functions that monads use.
// Essentially then you can wire up instance of thse domain abstractions using WireIn instead of Bind like this:
// .WireIn(new EnumerableMonad(lambda expression).
// And if you really want the exact same syntax as monads you can write a Bind function that just does that.
// This project starts out implementing monads, and then shows how they can be implemented in ALA.

// The IEnumerable monad has three implentations in this one project.
// Which one is used depends on two defines: IEnumerableMonad and ALA.
// 1) In the Monad folder in the Monad.List namespace, there is a conventional Bind implementaion using List<T>. It is immediate, which means the bind function evaluates as it goes.

// 2) In the Monad folder in the Monad.IEnumerable namespace, an implemntation that using Ienumerable, which is a deferred monad, so it doesn't get evaluated by the Binf function
//    but rather the Bind function sets up code to run later
//    There are two versions of the Bind function there, one that uses yield return (which makes the complier do all the real work of creating a new IEnumerable)
//    and one that doesn't use yield return. 
//    The one that doesn't use yield return is need for compainf with ALA. 
//    Note that while we think of monads as composing functions, deferred monad Bind functions actually return a structure built of objects.
//    This structure of objects consists of many compiler generated classed such as delgates and closures.

// 3) The ALA implementation of monads involves writing a domain abstraction that is pretty similar to the class needed for the monad implemenation

// To read this code, first take a look at the abstractions it uses. First look at the interfaces, classes and extension methods in EnumerableMonad.cs in the Monad subfolder. 
// Unless you are unfamiar with how monads work, you don't need to read the implementations.
// Then you should be able to read the Application method in this file.
// Notice how the first version uses the immediate version of the monad, the List. The Bind functions actually returns a result List immediately.

// Notice how the second version uses the deferred/pull version of the monad. The Bind function returns a program.
// The program is a reference to the last object in the chain because it is a pull monad.
// If we had implemented a deferred/push monad, that reference would have been the last object in the chain. 
// We make the program run be calling program.ToList() or by just using the return IEnumerable.

// Notice how the ALA version uses the same code as the monad version.
// I added a Bind extension method which we normally wouldn't bother with for ALA.
// With the Bind method added, I could prove that the ALA implementation allowed us to use identical syntax as monads for composing functions.
// But you can see it uses an ALA implementation because the using Monad.Enumerable namespace is taken out, and the using Domainabstractions namespace is in.
// If you compare the ALA version of the Enumerable.cs in the DomainAbstraction with the EnumerableMonad.cs in the Monad folder, you will see they are practically identical.
// The differences are:
//  1) The monad class used wires itself to its source via the constructor
//  2) The ALA domain abstraction has an extra interface added called WireForward. This allw the WireTo or WireIn operators to wire in the direction of the dataflow.

// So if ALA and monad can both compose functions, why use ALA?
// Its becasue ALA can do everything monads can do, but monads can't do everything ALA can do. ALA composes objects.
// This gives it much greater versatility, for example these objects can have many ports of different programming paradigms,
// and you can wire them up as an arbitrary graph. ALA is like an electrical circuit built with integrated circuits with many pins.
// Monads are more like a linear chain of resitor and capacitors.
// Monads can be thought of as ALA restricted to domain abstractions with one input port and one output port,
// where these two ports must be the same programming paradigm, and must be dataflows.
// Monads do sometimes have more than one input port, for example when merging two streams of data.
// Or if they are push style monads like iObservable, they can have more than one output subscriber. But that as far as the topology goes.


// uncomment one of the following variations
// #define ListMonad                        // demo of immediate monad using List. Bind is in the Monad.List namespace

// #define IEnumerableMonad                 // demo of deferred monad using IEnumerable. Bind is in Monad.Enumerable namespace
// #define ALAPullUsingWireIn               // demo of deferred monad using IEnumerable built using an ALA domain abstraction, but still wiring up using WireIn
// #define ALAPullUsingBind                // demo of deferred monad using IEnumerable built using an ALA domain abstraction, and Bind uses WireIn

// #define IObservableMonad                 // demo of deferred monad using IObserable. Bind is in Monad.ObservableMonad namespace
// #define IObservableMonad2                // demo of deferred monad using IObserable. Bind takes an Action parameter instead of a function
// #define ALAPushUsingWireIn               // demo of deferred monad using IObserable built on an ALA domain abstraction, but still Wiring using WireIn.
// #define ALAPushUsingBind                 // demo of deferred monad using IObserable built on an ALA domain abstraction, Bind uses WireIn.

// #define IEnumerableQuery                 // demo of using LINQ and ALA together
// #define IEnumerableQuery2                   // demo of using LINQ and ALA together simpler version for website
// #define IObservableQuery                 // demo or Reactive Extensions and ALA together

// #define IObservableChain                  // Chaining Domain abstractions that use iObservable as a port with monads
#define IObservableChainDynamic             // Chaining Domain abstractions that use iObservable<dynamic> as a port with monads



#if ListMonad
using Monad.List;
#endif

#if IEnumerableMonad
using Monad.Enumerable;
#endif

#if IObservableMonad
using Monad.ObservableMonad;
#endif

#if IObservableMonad2
using Monad.ObservableMonad2;
#endif

#if ALAPullUsingWireIn || ALAPushUsingWireIn || ALAPullUsingBind || ALAPushUsingBind || IEnumerableQuery || IEnumerableQuery2 || IObservableQuery || IObservableChain || IObservableChainDynamic
using DomainAbstractions;
#endif


using Foundation;
using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Collections;
using System.Reactive.Subjects;
using System.Dynamic;
using ProgrammingParadigms;

namespace Application
{
    class Program
    {
        // It just calls another function called Application
        // (if you don't do this then Main will either complete immediately (ending the program before the asyncronous tasks finish)
        // or if you put in a ConsoleReadKey or Thread.Sleep at the end of Main, that will just block the main thread, causing the asynchronous tasks to run on other threads.
        // The program still works when it uses other threads, but I particular wanted to demonstrate this monad working on a single thread.


        static int Main()
        {
            try
            {
                Debug.WriteLine("The application has started");
                Wiring.diagnosticOutput += (s) => Debug.WriteLine(s);
                Application();
                Debug.WriteLine("The application has finished");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }



        // The application function composes two functions using the List<T> or IEnumerable<T> monad.
        // The functions do the same thing, they take a number and produce three numbers that end in 1, 2 and 3.
        // So for example if the function receives 2, it will return 21, 22, and 23.
        // Note that the List and IEnumerable monads expand the number of items with each application of a function.
        // You can write Select and Aggregate functions that don't expand, but the monad function itself does expand the number of results.
        // A more practical example of the monad would say take a student and return all their courses.

        // First we create a source IEnumerable with a single value 0.
        // Then the function is binded in three times, so we end up with 27 numbers starting from 111, and ending with 333. 
        // The monad code in the monad or programming paradigms layer takes care of making everything work by providing two extension methiods, Bind() and ToTask().

#if ListMonad
        // Demo application that uses the typical implemention of List<T> Monad.
        // The Bind function can be found in the Monad namespace in the Monad subfolder in ListMonad.cs

        static void Application()
        {
            var result = new List<int> { 0 }  
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 })
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 })
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 });
            Console.WriteLine($"Final result is {result.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Library layer of this project where I put very abstract things that complement the .NEt library
        }
#endif



#if IEnumerableMonad || ALAPullUsingBind

        // Demo for IEnumerableMonad and demo for the ALA application both use the same application code here.
        // However different Bind functions are used from different namespaces: Monad.Enumerable and DomainAbstractions resp.
        // This is the same simple application as above, but uses the deferred implementation of the List<T> monad which is IEnumerable<T>.

        static void Application()
        {
            var program = new[] { 0 }  // start with an iEnumerable with one item
            .Bind(MutiplyBy10AndAdd1Then2Then3)
            .Bind(MutiplyBy10AndAdd1Then2Then3)
            .Bind(MutiplyBy10AndAdd1Then2Then3);

            Console.WriteLine(program.ObjectStructureToString());
            var result = program.ToList();  // now run the program
            Console.WriteLine($"Final result is {result.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Foundation layer of this project
        }


        // for the function that takes a number and returns three numbers ending in 1, 2 and 3,
        // I didn't want to use lists like I did in the List version above - I wanted lazy functions.
        // That meant creating a class that implements IEnumerable. 
        // The yield return syntax below is by far the easiest way to do that, as the compiler creates the needed class for you.
        // In the EnumerableMonad.cs file in the monad subfolder, you can see a class that implements IEnumerable without using yield return syntax.

        private static IEnumerable<int> MutiplyBy10AndAdd1Then2Then3(int x)
        {
            yield return x * 10 + 1;
            yield return x * 10 + 2;
            yield return x * 10 + 3;
        }
#endif




#if ALAPullUsingWireIn
        // Demo ALA application that does the same job as the IEnumerableMonad application above
        // The ALA application differs by using .WireInR and new keywords instead of .Bind.
        // It uses a Domain Abstraction class called EnumerableFunction, whichi is configured with a function returns many values, so it returns an IEnumerable

        static void Application()
        {
            var program = (IEnumerable<int>) new List<int> { 0 }
            .WireInR(new EnumerableFunction<int, int>(MutiplyBy10AndAdd1Then2Then3))
            .WireInR(new EnumerableFunction<int, int>(MutiplyBy10AndAdd1Then2Then3))
            .WireInR(new EnumerableFunction<int, int>(MutiplyBy10AndAdd1Then2Then3));
            var result = program.ToList();
            Console.WriteLine($"Final result is {result.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Foundation layer of this project
        }


        // for the IEnumerableMonad version of the composed function, (a function that takes a number and returns three numbers ending in 1, 2 and 3)
        // we don't want to return a list like we did in the List version above, we want to return a proper IEnumerable.
        // The yield return syntax is by far the easiest way to do that, as the compiler creates the needed class for you.
        // In the EnumerableMonad.cs file in the monad subfolder, you can see a class that implements iEnumerable without using yield return syntax.

        private static IEnumerable<int> MutiplyBy10AndAdd1Then2Then3(int x)
        {
            yield return x * 10 + 1;
            yield return x * 10 + 2;
            yield return x * 10 + 3;
        }
#endif





#if IObservableMonad || ALAPushUsingBind
        // The Demo for IObservableMonad and the demo for the ALA application both use the same application code here.
        // However different Bind function sin different namespaces are used: Monad.Observable and DomainAbstractions resp.
        // This demo is the IObservable equivalent of the previous iEnumerable application
        // The function that Bind takes is a function that takse an int and returns an IObservable<int>.

        static void Application()
        {
            // Demonstration of composing functions that take an int and return an IObservable<int> (MutiplyBy10AndAdd1Then2Then3)
            // using a Bind function function

            Observable.Create<int>(observer => { observer.OnNext(0); observer.OnCompleted();  return Disposable.Empty; })
            .Bind(MutiplyBy10AndAdd1Then2Then3)
            .Bind(MutiplyBy10AndAdd1Then2Then3)
            .Bind(MutiplyBy10AndAdd1Then2Then3)
            .Subscribe((x) => Console.Write($"{x} "),
                        (ex) => Console.Write($"Exception {ex}"),
                        () => Console.Write("Complete")
                        );
            Console.WriteLine();
            Console.WriteLine("Note: # are written out in Bind where it intercepted OnCompeted calls to effect joining all the observables into a single observable");
        }


        static IObservable<int> MutiplyBy10AndAdd1Then2Then3(int x)
        {
            return Observable.Create<int>(observer=>
            {
                observer.OnNext(x * 10 + 1);
                observer.OnNext(x * 10 + 2);
                observer.OnNext(x * 10 + 3);
                observer.OnCompleted();
                return Disposable.Empty;
            });
        }


#endif




#if IObservableMonad2
        static void Application()
        {
            // This Demo is the same as the previous, except that we have changed the signature of the function that Bind takes to making writing those functions simpler.
            // Instead of returning an IObservable<int> we pass it an IObserver<int>.
            // This alleviates the function from having to create an IObservable object, and that Object in turn having to receive an IObserver on its Subscribe.
            // Instead the function can immediately run and just put its output to the observer object that was passed to it up front.
            // If you look at the MutiplyBy10AndAdd1Then2Then3 function compared with the previous demo, it just immediately calls observer.OnNexts

            Observable.Create<int>(observer => { observer.OnNext(0); observer.OnCompleted();  return Disposable.Empty; })
            .Bind<int,int>(MutiplyBy10AndAdd1Then2Then3)
            .Bind<int,int>(MutiplyBy10AndAdd1Then2Then3)
            .Bind<int,int>(MutiplyBy10AndAdd1Then2Then3)
            .Subscribe((x) => Console.Write($"{x} "),
                        (ex) => Console.Write($"Exception {ex}"),
                        () => Console.Write("Complete")
                        );
        }


        static void MutiplyBy10AndAdd1Then2Then3(int x, IObserver<int> observer)
        {
                observer.OnNext(x * 10 + 1);
                observer.OnNext(x * 10 + 2);
                observer.OnNext(x * 10 + 3);
                observer.OnCompleted();
        }


#endif





#if ALAPushUsingWireIn
        // Demo ALA application that does the same job as the IObservableMonad2 application above
        // The ALA application differs by using .WireInR and new keywords instead of .Bind.
        // It uses a Domain Abstraction class called ObservableFunction, which is an abstraction to be configured with a function that returns many values, and so takes an IObservable for its output
        static void Application()
        {
            var outputer = (ObservableToOutput<int>)
            new ValueToObservable<int>(0)
            .WireInR(new ObservableFunction<int,int>(MutiplyBy10AndAdd1Then2Then3))
            .WireInR(new ObservableFunction<int, int>(MutiplyBy10AndAdd1Then2Then3))
            .WireInR(new ObservableFunction<int, int>(MutiplyBy10AndAdd1Then2Then3))
            .WireInR(new ObservableToOutput<int>(Console.Write));

            new StartEvent().WireTo(outputer)
                            .Run();
        }


        static IObservable<int> MutiplyBy10AndAdd1Then2Then3(int x)
        {
            return Observable.Create<int>(observer=>
            {
                observer.OnNext(x * 10 + 1);
                observer.OnNext(x * 10 + 2);
                observer.OnNext(x * 10 + 3);
                observer.OnCompleted();
                return Disposable.Empty;
            });
        }
#endif


        // We have now done seven demo applications that all do basically the same thing based on IEnumerable and Observable:
        // 1. IEnumerable Monad using Bind
        // 2. IEnumerable ALA using WireInR and new
        // 3. IEnumerable ALA using Bind to the WireInR and new to get the same exact application code as the monad versions (curiousity)
        // 4. IObservable Monad using Bind
        // 5. IObservable Monad using Bind that takes Actions that take an IObserver 
        // 6. IObservable ALA using WireInR and new
        // 7. IObservable ALA using Bind to the WireInR and new to get the same exact application code as the monad version (curiousity)
        




#if IEnumerableQuery
        // This domonstrates use of LINQ in an ALA application
        // The EnumerableQuery domain abstraction accepts a LINQ query as its configuration


        static void Application()
        {
            var proxySource1 = new EnumerableProxySource<int>();
            var query1 = proxySource1.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 1);
            var proxySource2 = new EnumerableProxySource<int>();
            var query2 = proxySource2.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 2);
            var proxySource3 = new EnumerableProxySource<int>();
            var query3 = proxySource3.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 3);


            // Now create an ALA program using the domain abstraction, Query
            var program = (EnumerableToConsoleOutput<int>)
            new List<int> { 0 }
            .WireInR(new EnumerableQuery<int, int>(proxySource1, query1) { instanceName = "Query1" })
            .WireInR(new EnumerableQuery<int, int>(proxySource2, query2) { instanceName = "Query2" })
            .WireInR(new EnumerableQuery<int, int>(proxySource3, query3) { instanceName = "Query3" })
            .WireInR(new EnumerableToConsoleOutput<int>());

            program.Run();

        }

        private static IEnumerable<int> MutiplyBy10AndAdd1Then2Then3(int x)
        {
            yield return x * 10 + 1;
            yield return x * 10 + 2;
            yield return x * 10 + 3;
        }



#endif

#if IEnumerableQuery2
        // This domonstrates use of LINQ in an ALA application
        // The EnumerableQuery domain abstraction accepts a LINQ query as its configuration
        // This time we use a simpler lambda expression for SelectMany


        static void Application()
        {
            var proxySource1 = new EnumerableProxySource<int>();
            var query1 = proxySource1.SelectMany(x => new[] { x * 10 + 1, x * 10 + 2, x * 10 + 3 }).Select(x => x + 1);
            var proxySource2 = new EnumerableProxySource<int>();
            var query2 = proxySource2.SelectMany(x => new[] { x * 10 + 1, x * 10 + 2, x * 10 + 3 }).Select(x => x + 2);
            var proxySource3 = new EnumerableProxySource<int>();
            var query3 = proxySource3.SelectMany(x => new[] { x * 10 + 1, x * 10 + 2, x * 10 + 3 }).Select(x => x + 3);


            // Now create an ALA program using the domain abstraction, Query
            var program = (EnumerableToConsoleOutput<int>)
            new List<int> { 0 }
            .WireInR(new EnumerableQuery<int, int>(proxySource1, query1) { instanceName = "Query1" })
            .WireInR(new EnumerableQuery<int, int>(proxySource2, query2) { instanceName = "Query2" })
            .WireInR(new EnumerableQuery<int, int>(proxySource3, query3) { instanceName = "Query3" })
            .WireInR(new EnumerableToConsoleOutput<int>());

            program.Run();

        }



#endif


#if IObservableQuery
        // This domonstrates use of LINQ in an ALA application
        // The ObservableQuery domain abstraction accepts a LINQ query as its configuration
        // Build the LINQ query starting with a subject
        // pass both the subject and the query to the domain abstraction's constructor to configure it
        static void Application()
        {
            var subject1 = new Subject<int>();
            var query1 = subject1.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 1);
            var subject2 = new Subject<int>();
            var query2 = subject2.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 2);
            var subject3 = new Subject<int>();
            var query3 = subject3.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 3);

            var program = (ObservableToConsoleOutput<int>)
            new ValueToObservable<int>(0)
            .WireInR(new ObservableQuery<int, int>(subject1, query1))
            .WireInR(new ObservableQuery<int, int>(subject2, query2))
            .WireInR(new ObservableQuery<int, int>(subject3, query3))
            .WireInR(new ObservableToConsoleOutput<int>());

            program.Run();
        }


        static IObservable<int> MutiplyBy10AndAdd1Then2Then3(int x)
        {
            return Observable.Create<int>(observer =>
            {
                observer.OnNext(x * 10 + 1);
                observer.OnNext(x * 10 + 2);
                observer.OnNext(x * 10 + 3);
                observer.OnCompleted();
                return Disposable.Empty;
            });
        }
#endif




#if IObservableChain
// Demo of mxing Domain abstractions that have IObservable ports with IObservable monads (reactive extensions)
// static version specifies an explicit type for the CSVFileReaderWriter.
// All other types along the dataflow are determined through type inference, even the ObservableToOutput at the end.

        static void Application()
        {
            var outputer =
            // new { Name = "Fred Person", Number = 99 }
            // .ToObservable()
            ((IObservable<DataType>)new CSVFileReaderWriter<DataType>() { FilePath = "DataFile1.txt" })
            .Select(x => new { Firstname = x.Name.SubWord(0), Number = x.Number + 1 })
            .Where(x => x.Number > 48)
            .ToOutput(Console.Write);

            var program = new StartEvent().WireTo(outputer);
            program.Run();
            program.Run();
        }


        private class DataType
        {
            public string Name { get; set; }
            public int Number { get; set; }
        }
#endif


#if IObservableChainDynamic
// Demo of mxing Domain abstractions that have IObservable ports with IObservable monads (reactive extensions)
// dynamic version. The CSV file must have header information. Use the write function of CSVFileReaderWriter to create a file with correct format first.
// The the demo proper is reading the file back and passing the data along the dataflow completely dynamicall. 
// The intial part of the dataflow uses ExpandoObject
// After the Select, it is an anonymous typed object and uses type inference for the rest of the way to the ObservableToOutput at the end.
// However the ObservableToOutput is effectively not typed because it takes object. This allows it to be Wired using WireInR rather than using an extension method to get type inference.

        static void Application()
        {
            var csvrw = new CSVFileReaderWriter() { FilePath = "DataFile2.txt" };
            IObserverPush<ExpandoObject> writer = csvrw;
            writer.OnStart();

            dynamic eo = new ExpandoObject();
            eo.Number = 47; 
            eo.Name = "Jack Up";
            writer.OnNext(eo);

            eo.Number = 48;
            eo.Name = "Wynn Takeall";
            writer.OnNext(eo);

            eo.Number = 49;
            eo.Name = "Rich Busted";
            writer.OnNext(eo);

            writer.OnCompleted();

            var outputer = new ObservableToOutput<object>(Console.Write);

            ((IObservable<dynamic>)csvrw)
                .Sniff()
                .Select(x => new { Firstname = ((string)x.Name).SubWord(0), Number = x.Number + 1 })
                .Sniff()
                .Where(x => x.Number > 48)
                .Sniff()
                .WireInR(outputer);

            var program = new StartEvent().WireTo(outputer);
            program.Run();
        }
#endif

    }

}





