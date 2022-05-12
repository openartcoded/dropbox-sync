using System;
using System.Collections.Generic;
using System.Text;

namespace DropboxSync.Helpers
{
    public static class StringHelper
    {
        public static string TrimFromChar(this string str, char charToRemove)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != charToRemove) break;

                str = str.Remove(i, 1);
                i--;
            }

            for (int i = str.Length - 1; i >= 0; i--)
            {
                if (str[i] != charToRemove) break;

                str = str.Remove(i, 1);
                i++;
            }

            return str;
        }
    }
}
