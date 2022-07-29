using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    static partial class LibraryExtensionMethods
    {
        // join extension method for IEnumerable<string>
        public static string Join(this IEnumerable<string> strings, string separator)
        {
            return strings.Aggregate(new StringBuilder(), (sb, s) => { if (sb.Length > 0) sb.Append(separator); sb.Append(s); return sb; }).ToString();
        }
    }
}
