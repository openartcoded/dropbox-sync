using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DropboxSync.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Regular expression of the character's forbidden in Windows's filename
        /// </summary>
        public const string FileRegEx = @".*[(\?\/\\\:\*\""\<\>\|)]";

        /// <summary>
        /// Remove <paramref name="charToRemove"/> from the start and the end of <paramref name="str"/>
        /// </summary>
        /// <param name="str">A neither <c>null</c> nor <c>empty</c> nor composed of a spaces string</param>
        /// <param name="charToRemove">The character to Trim</param>
        /// <returns><paramref name="str"/> trimmed of spaces and of <paramref name="charToRemove"/></returns>
        /// <exception cref="ArgumentNullException"
        public static string TrimFromChar(this string str, char charToRemove)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));

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
            }

            return str;
        }

        /// <summary>
        /// Check if <paramref name="str"/> matches de regular expression pattern defined in <see cref="FileRegEx"/>
        /// </summary>
        /// <param name="str">A neither null or empty string. It can't be composed of spaces only.</param>
        /// <returns><c>true</c> If <paramref name="str"/> match de regular expression pattern. <c>false</c> Otherwise.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool StringMatchFileRegEx(this string str)
        {
            str = str.Trim();

            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));

            Regex regex = new Regex(FileRegEx);

            return regex.IsMatch(str);
        }

        public static int KeepOnlyDigits(string message)
        {
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));

            string final = "";

            for (int i = 0; i < message.Length; i++)
            {
                if (char.IsDigit(message[i])) final += message[i];
            }

            if (!int.TryParse(final, out int finalNb))
                throw new Exception($"{nameof(final)} with value : \\{final}\\ could not be formatted to int");

            return finalNb;
        }
    }
}
