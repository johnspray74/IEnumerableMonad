using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    static partial class LibraryExtensionMethods
    {
        public static string SubWord(this string s, int index) 
        {
            if (s == null) return null;
            string[] subwords = s.Split(new[] { ' ', '.', ',' });
            if (index < subwords.Length) return subwords[index];
            return null;
        }
    }
}
