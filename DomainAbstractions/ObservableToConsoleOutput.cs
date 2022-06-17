using System;

namespace DomainAbstractions
{
    class ObservableToConsoleOutput<T>
    {
        private string separator;
        private string Separator { set { separator = value; } }

        private IObservable<T> source;   // input port

        // This gets called when source gets wired
        public void Run()
        { 
            source.Subscribe((x) => Console.Write($"{x} "),
                             (ex) => Console.Write($"Exception {ex}"),
                              () => Console.Write("Complete")
                    );
        }
    }
}
