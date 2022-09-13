using System;
using System.Collections.Generic;
using System.Linq;
using Library;

namespace DomainAbstractions
{



    class EnumerableToSerial<T>
    {
        private IEnumerable<T> input;  // input port 

        public event IEnumerableToSerialDelegate output;   // output port

        public EnumerableToSerial(IEnumerableToSerialDelegate output) { this.output = output; }

        public void Run()
        {
            if (input == null) throw new NullReferenceException();
            output?.Invoke($"Final result is {input.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Foundation layer of this project
        }
    }


    delegate void IEnumerableToSerialDelegate(string output);


}
