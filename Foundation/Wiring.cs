using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Foundation
{
    public static class Wiring
    {
        /// <Summary>
        /// Important method that wires and connects instances of classes that have ports by matching interfaces
        /// (with optional port name).
        /// WireTo is an extension method on the type object.
        /// If object A (this) has a private field of an interface, and object B implements the interface,
        /// then wire them together using reflection.
        /// The private field can also be a list.
        /// Returns this for fluent style programming.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="A">first object being wired</param>
        /// <param name="B">second object being wired</param>
        /// <param name="APortName">port fieldname in the A object (optional)</param>
        /// <returns>A</returns>
        /// ------------------------------------------------------------------------------------------------------------------
        /// WireTo method understanding what it does:
        /// <param name="A">
        /// The object on which the method is called is the object being wired from.
        /// It must have a private field of the interface type.
        /// </param> 
        /// <param name="B">
        /// The object being wired to. 
        /// It must implement the interface)
        /// </param> 
        /// <returns>this to support fluent programming style which allows multiple wiring to the same A object with .WireTo operators</returns>
        /// <remarks>
        /// 1. only wires compatible interfaces, e.g. A has a field of the type of an interface and B implements the interface
        /// 2. the field must be private (all publics are for use by the higher layer. This prevents confusion in the higher layer when when creating an instance of an abstraction - the ports should not be visible) 
        /// 3. can only wire a single matching port per call
        /// 4. Wires matching ports in the order they are decalared in class A (skips ports that are already wired)
        /// 5. looks for list as well (a list can block other ports of the same type lower down - they must be wired with an explict name)
        /// 6. you can overide the above order, or specify the port name explicitly, by giving the port field name in the WireTo method
        /// 7. After a successful wiring, looks for a method with the same name as the port but ending in the word "Initialize".
        ///    Be careful what you do in such a method as the wiring is not yet complete.
        ///    If the interface just wired contains a C# event, you can use it to register an event handler
        /// ------------------------------------------------------------------------------------------------------------------
        /// To get diagnostic output of all the wiring put one or more lines like this into your main function before any wiring is done.
        /// Wiring.diagnosticOutput += (s) => Debug.WriteLine(s);
        /// Wiring.diagnosticOutput += (s) => Console.WriteLine(s);
        /// Wiring.diagnosticOutput += new OutputToFile(@"C:\ProgramData\Example\wiringLog.txt").WriteLine;
        /// 
        /// </remarks>
        public static T WireTo<T>(this T A, object B, string APortName = null)
        {
            // achieve the following via reflection
            // A.field = B; 
            // provided 1) field is private,
            // 2) field's type matches one of the implemented interfaces of B, and
            // 3) field is not yet assigned

            if (A == null)
            {
                throw new ArgumentException("A is null ");
            }
            if (B == null)
            {
                throw new ArgumentException("B is null ");
            }
            bool wired = false;


            // first get a list of private fields in object A (matching the name if given) and an array of implemented interfaces of object B
            // do the reflection once
            var BType = B.GetType();
            var AfieldInfos = A.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Where(fi => fi.FieldType.IsInterface || fi.FieldType.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(fi.FieldType))   // filter for fields of interface type
                .Where(fi => APortName == null || fi.Name == APortName); // filter for given portname (if any) 
            var BinterfaceTypes = BType.GetInterfaces().ToList(); // do the reflection once


            // look through the private fields
            // (If multiple fields match, they are wired in the order they are declared)
            foreach (var AFieldInfo in AfieldInfos)
            {
                if (AFieldInfo.GetValue(A) == null)   // the private field is not yet assigned 
                {
                    // Is the field unassigned and type matches one of the interfaces of B
                    var BImplementedInterface = BinterfaceTypes.FirstOrDefault(interfaceType => AFieldInfo.FieldType == interfaceType);
                    if (BImplementedInterface != null)  // there is a matching interface
                    {
                        AFieldInfo.SetValue(A, B);  // do the wiring
                        wired = true;
                        diagnosticOutput?.Invoke(WiringToString(A, B, AFieldInfo));
                        Initialize(A, AFieldInfo);
                        break;
                    }
                }

                // Is the field a list whose generic type matches one of the interfaces of B
                var fieldType = AFieldInfo.FieldType;
                if (fieldType.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(fieldType))
                {
                    var AGenericArgument = AFieldInfo.FieldType.GetGenericArguments()[0];
                    var BImplementedInterface = BinterfaceTypes.FirstOrDefault(interfaceType => AGenericArgument.IsAssignableFrom(interfaceType));
                    if (BImplementedInterface != null)
                    {
                        var AListFieldValue = AFieldInfo.GetValue(A);
                        if (AListFieldValue == null)  // list not created yet
                        {
                            var listType = typeof(List<>);
                            Type[] listParam = { BImplementedInterface };
                            AListFieldValue = Activator.CreateInstance(listType.MakeGenericType(listParam));
                            AFieldInfo.SetValue(A, AListFieldValue);
                        }
                        // now add the B object to the list
                        AListFieldValue.GetType().GetMethod("Add").Invoke(AListFieldValue, new[] { B });
                        wired = true;
                        diagnosticOutput?.Invoke(WiringToString(A, B, AFieldInfo));
                        Initialize(A, AFieldInfo);
                        break;
                    }
                }
            }

            if (!wired) // throw exception
            {
                var AinstanceName = A.GetType().GetProperties().FirstOrDefault(f => f.Name == "InstanceName")?.GetValue(A);
                var BinstanceName = B.GetType().GetProperties().FirstOrDefault(f => f.Name == "InstanceName")?.GetValue(B);

                if (APortName != null)
                {
                    // a specific port was specified - see if the port was already wired
                    var AfieldInfo = AfieldInfos.FirstOrDefault();
                    if (AfieldInfo?.GetValue(A) != null) throw new Exception($"Port already wired {A.GetType().Name}[{AinstanceName}].{APortName} to {BType.Name}[{BinstanceName}]");
                }
                string AFieldsConsidered = string.Join(", ", AfieldInfos.Select(f => $"{f.Name}:{f.FieldType}, {(f.GetValue(A) == null ? "unassigned" : "assigned")}"));
                string BInterfacesConsidered = string.Join(", ", BinterfaceTypes.Select(f => $"{f}"));
                throw new Exception($"Failed to wire {A.GetType().Name}[{AinstanceName}].\"{APortName}\" to {BType.Name}[{BinstanceName}]. Considered fields of A: {AFieldsConsidered}. Considered interfaces of B: {BInterfacesConsidered}.");
            }
            return A;
        }

        /// <summary>
        /// Same as WireTo except that it return the second object to support composing a chain of instances of abstractions without nested syntax
        /// e.g. new A().WireIn(new B()).WireIn(new C());
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="A">first object being wired</param>
        /// <param name="B">second object being wired</param>
        /// <param name="APortName">port fieldname in the A object (optional)</param>
        /// <returns>B to support fluent programming style which allows wiring a chain of objects within .WireIn operators</returns>
        public static object WireIn<T>(this T A, object B, string APortName = null)
        {
            WireTo(A, B, APortName);
            return B;
        }


        // Some programming paradigm interfaces that we want to use need to be wired in the reverse direction from out average ALA push based dataflows.
        // The B object has the field and the A object implements it.
        // But the dataflow direction is from A to B, so we want to use WireTo or WireIn in the same direction as the dataflow.
        // These two Wiring methods reverse the wiring.
        // WireTo still returns the A object to support easy wiring of one object to many
        // WireIn still returns the B object to support chaining of operations

        public static object WireToR<T>(this T A, object B, string APortName = null)
        {
            WireTo(B, A, APortName);
            return A;
        }


        public static object WireInR<T>(this T A, object B, string APortName = null)
        {
            WireTo(B, A, APortName);
            return B;
        }


        // This method is called after we have done a succesfull wiring
        // It looks for a private method in the A object with the same name as the port's field with the word "Initialize" postfix
        // If it finds such a method it calls it.
        // These portnameInitialize methods are used in domain abstractions to any work that needs doing to complete the wiring, but no more than that as not all the wiring is complete yet
        // The canonical example is an interface that contains a C# event. At the A object end, an event hander typically needs to be registered to the event.
        // The intialize function can do that.


        private static void Initialize(object A, FieldInfo AFieldInfo)
        {
            // see if there is an Initialize function associated with the port and call it.
            var m = A.GetType().GetMethod($"{AFieldInfo.Name}Initialize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (m != null)
            {
                PostWiringInitializeDelegate handler = (PostWiringInitializeDelegate)Delegate.CreateDelegate(typeof(PostWiringInitializeDelegate), A, m);
                handler();
                diagnosticOutput?.Invoke($"Called initialize function in A: {AFieldInfo.Name}Initialize");

            }
        }

        public delegate void PostWiringInitializeDelegate();
 




        private static string WiringToString(object A, object B, FieldInfo matchedField)
        {
            var AClassName = A.GetType().Name;
            var BClassName = B.GetType().Name;
            var AInstanceName = "No InstanceName";
            var BInstanceName = "No InstanceName";
            var AInstanceNameField = A.GetType().GetField("InstanceName");
            var BInstanceNameField = B.GetType().GetField("InstanceName");
            if (AInstanceNameField != null) AInstanceName = (string)AInstanceNameField.GetValue(A);
            if (AInstanceNameField != null) AInstanceName = (string)AInstanceNameField.GetValue(A);
            return $"WireTo {AClassName}[{AInstanceName}].{matchedField.Name} ---> {BClassName}[{BInstanceName}] : {matchedField.FieldType}";
        }


        // diagnostics output port
        // doesn't have to be wired anywhere
        public delegate void DiagnosticOutputDelegate(string output);
        public static event DiagnosticOutputDelegate diagnosticOutput;

    }
}