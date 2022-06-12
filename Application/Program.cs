// This code is wrtten as example code for chapter 3 section on moands in the online book at abstractionlayeredarchitecture.com
// See that website for full discussion of the comparison between ALA and monads
// The maybe monad has three implentations in this one project.
// Which one is used depends on two defines: PushMonad and ALA.
// 1) In the Monad folder in the Monad.Maybe namespace, there is a conventional implementaion using IMaybe. It is immediate, which means the bind function evaluates as it goes.

// 2) In the Monad folder in the Monad.PushMaybe namespace, an implemntation that is deferred. A deferred monad is wired up by the Bind function for runnning later,
//    or for continuous data flow.
//    This version is needed to compare with ALA, which is always deferred. A deferred dataflow can be either pull or push driven. (similar to IEnumerable and IObservable.)
//    The one implemented here is a push monad because in ALA we default to pushing type dataflows, and only use pull when it makes more sense).
//    Note that while we think of monads as composing functions, deferred monads actually consist of a structure built of objects.
//    The Bind function operates on a left object and creates a right object.
//    If the monad works by pushing, the left object has a reference to the right object. Bind then returns the right object ready for the next Bind call.
//    The interface Imaybe, is implemented by the right object. This makes sense because data is pushed from left to right through the objects.
//    Therefore we need an interface that is implemented on the left object for the Bind function to use. This interface is called ISubscribeMaybe.
//    The Bind extension method is defined on this interface.
//    The ISubscribeMaybe and IMaybe interfaces are exactly analogous to the IObservable and IObserver interfaces.

// 3) ALA already comes with programming paradigms (which are almost the same thing as monad interfaces, same abstraction level).
//    And ALA is about composing objects (domain abstractions).
//    So it's really easy to implement monad like functionality.
//    All that is needed is a domain abstraction that's configurable via the constructor with a lambda expression of the type used by the respective monad.
//    Such a domain abstraction can be simply used like this: .WireIn(new Maybe(lambda expession))
//    This domain abstraction is Maybe.cs in the DomainAbstractions subfolder.
//    Then if we really want the same syntax as Monads, we can provide a Bind extension method which just does .WireIn(new Maybe(lambda expession)).
//    As with the deferred/push monad we described above, the interface, IMonad is implemented by the right monad object, not the left.
//    So the Bind extension method can't be defined on that interface. Instead we define Bind on a dummy interface called IBindable.
//    IBindable is like ISubscribemaybe that we had to use for the deferred/push monad, except it doesn't need a subscribe method in it. IBandable contains no methods. 
//    The Bind function doesn't need a subscribe method, it just uses WireIn instead, becasue that's how all domain abstractions are wired.

// To read this code, first take a look at the abstractions it uses. First look at the interfaces, classes and extension methods in Maybe.cs in the Monad subfolder. 
// Unless you are unfamiar with how monads work, you don't need to read the implementations.
// Then you should be able to read the Application method in this file.
// Notice how the first version uses the immediate version of the monad. The Bind functions actually returns a result immediately

// Notice how the second version uses the deferred/push version of the monad. The Bind function returns a program.
// The program is a reference to the first object in the chain (the ToMaybe object) because it is a push monad.
// If we had implemented a deferred/pull monad, that reference would have been the last object in the chain. 
// We make the program run be calling program.Run().

// Notice how the ALA version uses the same code as the monad version. I added a Bind extension method (and its IBindable interface) which we normally would other with for ALA.
// With the Bind method added, I could prove that the ALA implementation allowed us to use identical syntax.
// But you can see it uses an ALA implementation because the using Monad.PushMaybe namespace is taken out, and the using Domainabstractions namespace is in.
// If you compare the ALA version of the Maybe.cs in the DomainAbstraction with the Maybe.cs in the Monad folder, you will see they are practically identical.
// The differences are:
//  1) The ALA Bind function uses WireIn whereas the monad Bind function uses the subscribe method on the ISubscribeMaybe interface.
//  2) The ALA domain abstraction does implements IBinadable (which is an empty interface) instead of iSubscribeMaybe

// If you feel that the pushing version of the monad we have implemented here is not a real monad because the composed functions are not returning an interface,
// they are of the form Action<T, IMonad<U>, well it's ok, it's just semantics.
// We could just as easily implement a deferred pull monad which does compose functions of the form I -> IMaybe<U>,
// and still the ALA and monad implementations would be practically identical.

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
        // Main just uses the Nito.AsyncEx package to proovde a dispatcher to enable console programs to use async/await on one thread.
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



        // The application function composes two functions using the Continuation monad.
        // The Continuation monad consists of Task<T> plus the Totask extension method plus the Bind extension method (both of which are defined in the programming paradigms layer folder)
        // First it creates a source Task with the value 1. Then the first function adds 2, then the second function adds a number from the console. 
        // The thing is, because we are using the Coninuation monad, these two functions are allowed to take as much time as they want (be asynchronous).
        // To demo that, the first function does a delay, and the second function waits for Console input.
        // What the monad does is, despite these functions taking time and returning a Task object instead of an immediate result, allows you to compose them as if they were functions that just return a result. You just have to use the Bind function to compose them.
        // And the function must return Task<U> instead of U.
        // The monad code in the programming paradigms layer takes care of making everything work by providing two extension methiods, Bind() and ToTask().
        // There is no blocking of the main thread in this program until it hits the final ReadKey.
        // Everything runs on a single thread.

        // There are two versions - the first version uses async/await. If you are not familiar with async/await then use the second version which uses ContinueWith instead.
        // Each version has a verbose version that Console.WriteLines stuff to see what is going on.

#if !IEnumerableMonad && !ALA


        // This is a simple sample application that uses the typical implemention of maybe Monad.

        static void Application()
        {
            var result = new List<int> { 1, 2, 3 }
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 })
            .Bind(x => new List<int> { x * 10 + 1, x * 10 + 2, x * 10 + 3 });
            Console.WriteLine($"Final result is {result.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Foundation layer of this project
        }

#else // IEnumerable or ALA version

        // This is the same simple application as above, but uses deferred implementation of the List monad which is IEnumerable.

        static void Application()
        {
            var result = Return1Then2Then3()
#if ALA
            .ToWireableEnumerable()
#endif
            .Bind(x => MutiplyBy10AndAdd1Then2Then3(x))
            .Bind(x => MutiplyBy10AndAdd1Then2Then3(x));
            Console.WriteLine($"Final result is {result.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Foundation layer of this project
        }

        private static IEnumerable<int> Return1Then2Then3()
        {
            yield return 1;
            yield return 2;
            yield return 3;
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



