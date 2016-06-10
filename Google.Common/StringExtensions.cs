using System;
using System.Linq;

namespace Google.Common
{
    public static class StringExtensions
    {
        public static bool ContainsAlphaOnly(this string value)
        {
            return value.All(Char.IsLetter);
        }

        public static string RemoveSections(this string value, char sectionStartChar, char sectionEndChar)
        {
            char[] array = new char[value.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < value.Length; i++)
            {
                char let = value[i];
                if (let == sectionStartChar)
                {
                    inside = true;
                    continue;
                }
                if (let == sectionEndChar)
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
    }
}
