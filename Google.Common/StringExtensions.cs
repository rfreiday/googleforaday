using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Google.Common
{
    public static class StringExtensions
    {
        public static bool ContainsAlphaOnly(this string value)
        {
            return value.All(Char.IsLetter);
        }
    }
}
