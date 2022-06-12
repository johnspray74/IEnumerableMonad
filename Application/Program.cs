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



// #define IEnumerableMonad
#define ALA              // Selects ALA version designed to run identical application layer


#if ALA
using DomainAbstractions;
#else
#if IEnumerableMonad
using Monad.Enumerable;
#else
using Monad.List;
#endif
#endif
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
#if ALA
                Wiring.diagnosticOutput += (s) => Debug.WriteLine(s);
#endif
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

#if !IEnumerableMonad && !ALA


        // This is a simple sample application that uses the typical implemention of maybe Monad.

        static void Application()
        {
            var result = new List<int> { 0 }
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 })
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 })
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 });
            Console.WriteLine($"Final result is {result.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Foundation layer of this project
        }

#else // IEnumerable or ALA version

        // This is the same simple application as above, but uses deferred implementation of the List monad which is IEnumerable.

        static void Application()
        {
            var program = new List<int> { 0 }
#if ALA
            .ToWireableEnumerable()
#endif
            .Bind(x => MutiplyBy10AndAdd1Then2Then3(x))
            .Bind(x => MutiplyBy10AndAdd1Then2Then3(x))
            .Bind(x => MutiplyBy10AndAdd1Then2Then3(x));

            var result = program.ToList();
            Console.WriteLine($"Final result is {result.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Foundation layer of this project
        }


        private static IEnumerable<int> MutiplyBy10AndAdd1Then2Then3(int x)
        {
            yield return x * 10 + 1;
            yield return x * 10 + 2;
            yield return x * 10 + 3;
        }

#endif
}

}



