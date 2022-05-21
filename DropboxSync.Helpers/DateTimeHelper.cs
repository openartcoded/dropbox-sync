using System;
using System.Collections.Generic;
using System.Text;

namespace DropboxSync.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime FromUnixTimestamp(long timestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0);
            double v = Convert.ToDouble(timestamp);
            dateTime = dateTime.AddMilliseconds(v);

            return dateTime;
        }
    }
}
