﻿// This code is written as example code for chapter 6 section on monads in the on-line book at abstractionlayeredarchitecture.com
// See that website for full discussion of the comparison between ALA and monads
// and simplified code snippets taken from this project

// In brief, both ALA and monads are 'composition' patterns.
// ALA composes objects and monads compose functions.
// ALA is a more general solution for composition. It is also easier to understand because it uses plain objects.
// Monads, at least the deferred type of monads, generate a lot of code under the covers, which makes them difficult to understand and explain.
// Because ALA is a more general solution, we can implement monad behaviour (composition of functions) using ALA.
// This sample code starts with simple non-defferred monads and works through deferred monads and finally doing the equivalent functionality of monads in ALA.
// Finally we show how an existing monad library such as LINQ or RXs can be used with ALA.
// We do this for both IEnumerable and IObservable based monads.

// The different examples in this project are controlled by conditional compiling using defines, which are just below.
// The defines bring in different namespaces so that the examples use either ALA DomainAbstractions and ProgrammingParadigms namespace or the relevant Bind function in the relavant Monad namespace.




// uncomment one of the following variations
// #define ListMonad                        // demo of immediate monad using List. Bind is in the Monad.List namespace

// #define IEnumerableMonad                 // demo of deferred monad using IEnumerable. Bind is in Monad.Enumerable namespace
// #define ALAPullUsingIEnumerable          // demo of ALA using IEnumerable as the programming paradigm interface
// #define ALAPullUsingIEnumerableAndBind   // demo of ALA using IEnumerable but with Bind syntax the same as the monad version

// #define IObservableMonadF                // demo of deferred monad using IObserable. Bind takes a Func. Bind is in Monad.ObservableMonadF namespace
// #define IObservableMonadA                // demo of deferred monad using IObserable. Bind takes an Action. Bind is in Monad.ObservableMonadA namespace
// #define ALAPushUsingIObservable          // demo of ALA using IObserable  as the programming paradigm interface.
// #define ALAPushUsingIObservableAndBind   // demo of ALA using IObserable but with syntax identical to the monad version
// #define ALAPushUsingIObserverPush        // demo of ALA using IObserverPush as the programming paradigm interface
// #define ALAPushUsingIObserverPushAndBind // demo of ALA using IObserverPush but with Bind syntax the same as the monad version

// #define ALAWithIEnumerableQuery                 // demo of using LINQ and ALA together by using a domain abstraction that is configured with a LINQ Query
// #define ALAWithIEnumerableQuery2                // demo of using LINQ and ALA together simpler version for website
// #define ALAWithIObservableQuery                 // demo of using Reactive Extensions and ALA together by using a domain abstraction that is configured with an RX expression

// #define ALAWithIObservableChain                 // Chaining Domain abstractions that use iObservable as a port with monads
#define ALAWithIObservableChainDynamic          // Chaining Domain abstractions that use iObservable<dynamic> as a port with monads



// To read this code, first take a look at the abstractions it uses in the relevant Monad or ProgrammingParadigms & DomainAbstractions subfolders.
// Just look at the abstractions themselves, not the implementations.
// Then you should be able to read the Application code in this file.


// So if ALA and monad can both compose functions, why use ALA?
// Its becasue ALA can do everything monads can do, but monads can't do everything ALA can do. ALA composes objects.
// This gives it much greater versatility, for example these objects can have many ports of different programming paradigms,
// and you can wire them up as in an arbitrary graph. ALA is like an electrical circuit built with integrated circuits with many pins.
// Monads are more like a linear chain of resitor and capacitors.
// Monads can be thought of as ALA restricted to domain abstractions with one input port and one output port,
// where these two ports must be the same programming paradigm, and must be dataflows.
// (Monads do sometimes have more than one input port, for example when merging two streams of data.
// Or if they are push style monads like iObservable, they can have more than one output subscriber. But that is as far as the topology goes.)



#if ListMonad
using Monad.List;
#endif

#if IEnumerableMonad
using Monad.Enumerable;
#endif

#if IObservableMonadF
using Monad.ObservableMonadF;
#endif

#if IObservableMonadA
using Monad.ObservableMonadA;
#endif

#if ALAPullUsingIEnumerable || ALAPushUsingIObservable || ALAPullUsingIEnumerableAndBind || ALAPushUsingIObservableAndBind || ALAWithIEnumerableQuery || ALAWithIEnumerableQuery2 || ALAWithIObservableQuery || ALAWithIObservableChain || ALAWithIObservableChainDynamic || ALAPushUsingIObserverPush || ALAPushUsingIObserverPushAndBind
using DomainAbstractions;
using ProgrammingParadigms;
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



        // The different version sof the application all compose three functions.
        // For the sake of brevity, the functions do the same thing, they take a number and produce three numbers that end in 1, 2 and 3.
        // So for example if the function receives 2, it will return 21, 22, and 23.
        // Each time we compose the function, the number of items increases (Bind is the same as SelectMany).
        // You can write Select and Aggregate functions that don't expand, but the monad function itself does expand the number of results.
        // A more practical example of the monad would say take a student and return all their courses.

        // First we always create a starting single value with value 0.
        // Then the function is binded in three times, so we end up with 27 numbers starting from 111, and ending with 333. 


#if ListMonad
        // Demo application that uses the List<T> Monad.
        // The functions being composed are the lambda expressions.
        // The Bind function can be found in the Monad namespace in the Monad subfolder in ListMonad.cs
        // Notice how the List<T> monad is an immediate monad - the Bind function actually returns the result.

        static void Application()
        {
            var result = new List<int> { 0 }  
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 })
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 })
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 });
            Console.WriteLine($"Final result is {result.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Library layer of this project where I put very abstract things that complement the .NEt library
        }


        // Prints the following to the console:
        // Final result is 111 112 113 121 122 123 131 132 133 211 212 213 221 222 223 231 232 233 311 312 313 321 322 323 331 332 333
#endif




#if IEnumerableMonad || ALAPullUsingIEnumerableAndBind
        // Demo application that uses the IEnumerableMonad
        // This is the same simple application as above, but uses the deferred implementation of the List<T> monad which is IEnumerable<T>.

        // Becasue this si a deferred monad, the Bind function returns a program which you can subsequently run.
        // The program is a reference to the last object in the chain because it is a pull monad.
        // We make the program run be calling program.ToList() or by just using the returned IEnumerable in a foreach.


        static void Application()
        {
            var startValue = new[] { 0 };
            var program = startValue
            .Bind(MutiplyBy10AndAdd1Then2Then3)
            .Bind(MutiplyBy10AndAdd1Then2Then3)
            .Bind(MutiplyBy10AndAdd1Then2Then3);

            Console.WriteLine(program.ObjectStructureToString());  // print out the object structure of the program

            var result = program.ToList();  // now run the program
            Console.WriteLine($"Final result is {result.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Foundation layer of this project

            // we run deferred monads twice to ensure they work repeatedly
            result = program.ToList();  
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

        // Notice how ALA can be made to use exactly the same application code as the monad version.
        // To do this I added a Bind extension method to the IEnumerableFunction domain abstraction which we normally do in ALA.
        // That Bind function simple instantiates the domain abstraction and calls WireIn().
        // You can see the Bind function is different for the ALA version because its using Domainabstractions namespace instead of the Monad.Enumerable namesapce.
        // If you compare the ALA version of the EnumerableFunction.cs in the DomainAbstraction with the EnumerableMonad.cs in the Monad folder,
        // You will see they are practically identical. 
        // The only difference is that the monad version does its own wiring via the constructor whereas the ALA domain abstraction source variable is a port which WireInR sets.
#endif




#if ALAPullUsingIEnumerable
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
            result = program.ToList();
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





#if IObservableMonadF || ALAPushUsingIObservableAndBind
        // The Demo for IObservableMonadF and the demo for the ALA application both use the same application code here.
        // However different Bind function sin different namespaces are used: Monad.Observable and DomainAbstractions resp.
        // This demo is the IObservable equivalent of the previous iEnumerable application
        // The function that Bind takes is a function that takse an int and returns an IObservable<int>.

        static void Application()
        {
            // Demonstration of composing functions that take an int and return an IObservable<int> (MutiplyBy10AndAdd1Then2Then3)
            // using a Bind function function

            var program =
            Observable.Create<int>(observer => { observer.OnNext(0); observer.OnCompleted(); return Disposable.Empty; })
            .Bind(MutiplyBy10AndAdd1Then2Then3)
            .Bind(MutiplyBy10AndAdd1Then2Then3)
            .Bind(MutiplyBy10AndAdd1Then2Then3);
            program.Subscribe((x) => Console.Write($"{x} "),
                        (ex) => Console.Write($"Exception {ex}"),
                        () => Console.WriteLine("Complete")
                        );
            program.Subscribe((x) => Console.Write($"{x} "),
                        (ex) => Console.Write($"Exception {ex}"),
                        () => Console.WriteLine("Complete")
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




#if IObservableMonadA
        static void Application()
        {
            // This Demo is the same as the previous, except that we have changed the signature of the function that Bind takes to making writing those functions simpler.
            // Instead of returning an IObservable<int> we pass it an IObserver<int>.
            // This alleviates the function from having to create an IObservable object, and that Object in turn having to receive an IObserver on its Subscribe.
            // Instead the function can immediately run and just put its output to the observer object that was passed to it up front.
            // If you look at the MutiplyBy10AndAdd1Then2Then3 function compared with the previous demo, it just immediately calls observer.OnNexts
            // Note that this syntactically doesn't quite acheive the same as the monad version because of the .Bind<int,int> instead of just .Bind

            var program = Observable.Create<int>(observer => { observer.OnNext(0); observer.OnCompleted(); return Disposable.Empty; })
            .Bind<int, int>(MutiplyBy10AndAdd1Then2Then3)  // type inference wont work
            .Bind<int, int>(MutiplyBy10AndAdd1Then2Then3)
            .Bind<int, int>(MutiplyBy10AndAdd1Then2Then3);
            program.Subscribe((x) => Console.Write($"{x} "),
                        (ex) => Console.Write($"Exception {ex}"),
                        () => Console.WriteLine("Complete")
                        );
            program.Subscribe((x) => Console.Write($"{x} "),
                        (ex) => Console.Write($"Exception {ex}"),
                        () => Console.WriteLine("Complete")
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





#if ALAPushUsingIObservable
        // Demo ALA application that does the same job as the IObservableMonadA application above
        // The ALA application differs by using .WireInR and new keywords instead of .Bind.
        // It uses a Domain Abstraction class called ObservableFunction, which is an abstraction to be configured with a function that returns many values, and so takes an IObservable for its output
        static void Application()
        {
            var outputer = (ObservableToSerial<int>)
            new ValueToObservable<int>(0)
            .WireInR(new ObservableFunction<int,int>(MutiplyBy10AndAdd1Then2Then3))
            .WireInR(new ObservableFunction<int, int>(MutiplyBy10AndAdd1Then2Then3))
            .WireInR(new ObservableFunction<int, int>(MutiplyBy10AndAdd1Then2Then3))
            .WireInR(new ObservableToSerial<int>(Console.Write));

            var program = new StartEvent().WireTo(outputer);
            program.Run();
            // run the program twice to prove that none of the IObservables are locked up, nor get double subscribed
            program.Run();
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



#if ALAPushUsingIObserverPush
        // Demo ALA application using a domain abstraction called ObserverPushAction which has IObserverPush ports and does similar job to a monad.
        // You configure the ObserverPushAction with an action that takes a T and an IObserver<U> for it to output to.
        static void Application()
        {
            var program = new StartEvent();
            program
            .WireIn(new ValueToObserverPush<int>(0))
            .WireIn(new ObserverPushAction<int,int>(MutiplyBy10AndAdd1Then2Then3))
            .WireIn(new ObserverPushAction<int, int>(MutiplyBy10AndAdd1Then2Then3))
            .WireIn(new ObserverPushAction<int, int>(MutiplyBy10AndAdd1Then2Then3))
            .WireIn(new ObserverPushToSerial<int>(Console.Write));

            program.Run();
            program.Run();
        }


        static void MutiplyBy10AndAdd1Then2Then3(int x, IObserver<int> output)
        {
            output.OnNext(x * 10 + 1);
            output.OnNext(x * 10 + 2);
            output.OnNext(x * 10 + 3);
            output.OnCompleted();
        }
#endif





#if ALAPushUsingIObserverPushAndBind
        // Demo ALA application using a domain abstraction called ObserverPushAction which has IObserverPush ports and does similar job to a monad.
        // You configure the ObserverPushAction with an action that takes a T and an IObserver<U> for it to output to.
        // This is same as previous version but uses a Bind function instead of .WireIn(new ObserverPushAction<int, int>(MutiplyBy10AndAdd1Then2Then3))
        // Note that this syntactically doesn't quite acheive the same as the monad version for two reasons:
        // 1. We end up having to use Bind<int,int> instead of just Bind because the compiler can't infer the Bind's U type when the second parameter is an action (surprizingly it worked for functions that return IObservable<U> as we did above).
        //    The only workaround I could find was to add a dummy U type parameter to Bind and pass it a dummy value of type U, which is still syntatically deficient. 
        //    The second two Binds that pass in a 0 use this method. Bind needed an overload that takes three parameters to make this work - its completely ugly.
        // 2. We need to cast ValueToObservePush to IBindable before we can use Bind on it
        // So basically in ALA we fail to reproduce monad .Bind syntax when composing Action instead of a Func. Actions are not really used in real monads anyway.
        static void Application()
        {
            var program = new StartEvent();
            ((IBindable<int>)program.WireIn(new ValueToObserverPush<int>(0)))
            .Bind<int, int>(MutiplyBy10AndAdd1Then2Then3)  // type inference wont work
            .Bind(0, MutiplyBy10AndAdd1Then2Then3)  // alternative to Bind<int,int> that allows type inference to work
            .Bind(0, MutiplyBy10AndAdd1Then2Then3)
            .WireIn(new ObserverPushToSerial<int>(Console.Write));

            program.Run();
            program.Run();
        }

        static void MutiplyBy10AndAdd1Then2Then3(int x, IObserver<int> output)
        {
            output.OnNext(x * 10 + 1);
            output.OnNext(x * 10 + 2);
            output.OnNext(x * 10 + 3);
            output.OnCompleted();
        }
#endif






#if ALAWithIEnumerableQuery
        // This domonstrates use of LINQ in an ALA application
        // The EnumerableQuery domain abstraction accepts a LINQ query as its configuration
        // The programming paradigm interface is IEnumerable

        static void Application()
        {
            var proxySource1 = new EnumerableProxySource<int>();
            var query1 = proxySource1.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 1);
            var proxySource2 = new EnumerableProxySource<int>();
            var query2 = proxySource2.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 2);
            var proxySource3 = new EnumerableProxySource<int>();
            var query3 = proxySource3.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 3);


            var userStory = new StartEvent();
            userStory
            .WireIn(new ValueToDataFlow<int>(0))
            .WireIn(new EnumerableQuery<int, int>(proxySource1, query1) { instanceName = "Query1" })
            .WireIn(new EnumerableQuery<int, int>(proxySource2, query2) { instanceName = "Query2" })
            .WireIn(new EnumerableQuery<int, int>(proxySource3, query3) { instanceName = "Query3" })
            .WireIn(new DataFlowToSerial<int>(Console.Write));

            userStory.Run();
            Console.WriteLine();
            userStory.Run();  // Make sure the user story can be rerun
        }

        private static IEnumerable<int> MutiplyBy10AndAdd1Then2Then3(int x)
        {
            yield return x * 10 + 1;
            yield return x * 10 + 2;
            yield return x * 10 + 3;
        }



#endif

#if ALAWithIEnumerableQuery2
        // This domonstrates use of LINQ in an ALA application
        // The EnumerableQuery domain abstraction accepts a LINQ query as its configuration
        // unlike the version above, this version uses a lambda expression for SelectMany
        // The programming paradigm interface is IDataFlow


        static void Application()
        {
            var proxySource1 = new EnumerableProxySource<int>();
            var query1 = proxySource1.SelectMany(x => new[] { x * 10 + 1, x * 10 + 2, x * 10 + 3 }).Select(x => x + 1);
            var proxySource2 = new EnumerableProxySource<int>();
            var query2 = proxySource2.SelectMany(x => new[] { x * 10 + 1, x * 10 + 2, x * 10 + 3 }).Select(x => x + 2);
            var proxySource3 = new EnumerableProxySource<int>();
            var query3 = proxySource3.SelectMany(x => new[] { x * 10 + 1, x * 10 + 2, x * 10 + 3 }).Select(x => x + 3);


            // Now create an ALA program using the domain abstraction, Query
            var userStory = new StartEvent();
            userStory
            .WireIn(new ValueToDataFlow<int>(0))
            .WireIn(new EnumerableQuery<int, int>(proxySource1, query1) { instanceName = "Query1" })
            .WireIn(new EnumerableQuery<int, int>(proxySource2, query2) { instanceName = "Query2" })
            .WireIn(new EnumerableQuery<int, int>(proxySource3, query3) { instanceName = "Query3" })
            .WireIn(new DataFlowToSerial<int>(Console.Write));

            userStory.Run();
            Console.WriteLine();
            userStory.Run();  // Make sure the user story can be rerun
        }



#endif



#if ALAWithIObservableQuery
        // This domonstrates use of LINQ in an ALA application
        // The ObservableQuery domain abstraction accepts a LINQ query as its configuration
        // Build the LINQ query starting with a subject
        // Pass both the subject and the query to the ObservableQuery's constructor to configure it.
        // ObservableQuerys can be wired in a chain because they use the IObserverPush programming paradigm.
        // IObserverPush is like IObserver but doesn't use IObservable to subscribe, instead it is wired in the normal ALA way and has a OnStart method to allow it to restart after OnCompleted or OnError
        // Unlike the version above, this version uses a named function for the SelectMany
        static void Application()
        {
            var subject1 = new Subject<int>();
            var query1 = subject1.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 1);
            var subject2 = new Subject<int>();
            var query2 = subject2.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 2);
            var subject3 = new Subject<int>();
            var query3 = subject3.SelectMany(MutiplyBy10AndAdd1Then2Then3).Select(x => x + 3);

            var userStory = new StartEvent();
            userStory
            .WireIn(new ValueToObserverPush<int>(0))
            .WireIn(new ObservableQuery<int, int>(subject1, query1))
            .WireIn(new ObservableQuery<int, int>(subject2, query2))
            .WireIn(new ObservableQuery<int, int>(subject3, query3))
            .WireIn(new ObserverPushToSerial<int>(Console.Write));

            userStory.Run();
            userStory.Run();  // test we can run it twice
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

        static IObservable<int> MutiplyBy10AndAdd1Then2Then3ThenError(int x)
        {
            return Observable.Create<int>(observer =>
            {
                observer.OnNext(x * 10 + 1);
                observer.OnNext(x * 10 + 2);
                observer.OnNext(x * 10 + 3);
                observer.OnError(new Exception("No reason"));
                return Disposable.Empty;
            });
        }
#endif




#if ALAWithIObservableChain
        // Demo of mxing Domain abstractions that have IObservable ports with IObservable monads (reactive extensions)
        // static version specifies an explicit type for the CSVFileReaderWriter.
        // All other types along the dataflow are determined through type inference, even the ObservableToSerial at the end.

        static void Application()
        {
            var csvrw = new CSVFileReaderWriter<DataType>() { FilePath = "DataFile1.txt" };

            // First write something to the CSV file
            IObserverPush<DataType> writer = csvrw;
            writer.OnStart();
            writer.OnNext(new DataType() { Number = 47, Name = "Jack Up" });
            writer.OnNext(new DataType() { Number = 48, Name = "Wynn Takeall" });
            writer.OnNext(new DataType() { Number = 49, Name = "Rich Busted" });
            writer.OnCompleted();


            // Create a program for reading the CSV file and displaying it on the console

            var outputer =
            ((IObservable<DataType>)csvrw)
            .Select(x => new { Firstname = x.Name.SubWord(0), Number = x.Number + 1 })
            .Where(x => x.Number > 48)
            .ToSerial(Console.Write); // need to use ToSerial instead of WireInR(new ObservableToSerial<T>(Console.Write)) because T needs to be determined by TypeInference from the output of the Where

            var userStory = new StartEvent().WireTo(outputer);


            userStory.Run();
            userStory.Run();   // run the user story twice to show it can rerun
        }


        private class DataType
        {
            public string Name { get; set; }
            public int Number { get; set; }
        }
#endif





#if ALAWithIObservableChainDynamic
        // Demo of mxing Domain abstractions that have IObservable ports with IObservable monads (reactive extensions)
        // dynamic version. The CSV file must have header information. Use the write function of CSVFileReaderWriter to create a file with correct format first.
        // The the demo proper is reading the file back and passing the data along the dataflow completely dynamicall. 
        // The intial part of the dataflow uses ExpandoObject
        // After the Select, it is an anonymous typed object and uses type inference for the rest of the way to the ObservableToSerial at the end.
        // However the ObservableToSerial is effectively not typed because it takes object. This allows it to be Wired using WireInR rather than using an extension method to get type inference.

        static void Application()
        {
            var csvrw = new CSVFileReaderWriter() { FilePath = "DataFile2.txt" };

            // First write something to the CSV file
            IObserverPush<ExpandoObject> writer = csvrw;
            writer.OnStart();
            dynamic eo = new ExpandoObject(); 
            eo.Number = 47; eo.Name = "Jack Up"; writer.OnNext(eo);
            eo.Number = 48; eo.Name = "Wynn Takeall"; writer.OnNext(eo);
            eo.Number = 49; eo.Name = "Rich Busted"; writer.OnNext(eo);
            writer.OnCompleted();

            // Create a program for reading the CSV file and displaying it on the console

            var outputer = new ObservableToSerial<object>(Console.Write);

            ((IObservable<dynamic>)csvrw)
                // .Sniff()
                .Select(x => new { Firstname = ((string)x.Name).SubWord(0), Number = x.Number + 1 })
                // .Sniff()
                .Where(x => x.Number > 48)
                // .Sniff()
                .WireInR(outputer);

            var program = new StartEvent().WireTo(outputer);


            program.Run();  
            program.Run();  // run the program twice to make sure it can rerun
        }
#endif

    }

}





