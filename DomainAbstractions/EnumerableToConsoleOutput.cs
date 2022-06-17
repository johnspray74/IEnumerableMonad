using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;

namespace DomainAbstractions
{



    class EnumerableToConsoleOutput<T>
    {
        private IEnumerable<T> input;  // input port 


        public void Run()
        {
            if (input == null) throw new NullReferenceException();
            Console.WriteLine($"Final result is {input.Select(x => x.ToString()).Join(" ")}");  // This Join comes from the Foundation layer of this project
        }
    }
}
