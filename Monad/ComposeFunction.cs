using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComposeFunction
{
    static partial class ExtensionMethods
    {
        public static Func<int> ToFunc(this int source)
        {
            return () => source;
        }

        public static Func<int> Compose(this Func<int> source, Func<int, int> function)
        {
            return () => function(source());
        }
    }
}
