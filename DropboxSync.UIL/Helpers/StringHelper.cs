using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Helpers
{
    public static class StringHelper
    {
        public static int KeepOnlyDigits(string message)
        {
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
